using Autofac;
using BitMono.API.Protecting;
using BitMono.Core.Configuration.Dependencies;
using BitMono.Host;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ILogger = BitMono.Core.Logging.ILogger;

public class Program
{
    const string EncryptionFile = nameof(BitMono) + "." + nameof(BitMono.Encryption) + ".dll";
    const string ProtectionsFile = nameof(BitMono) + "." + nameof(BitMono.Protections) + ".dll";

    private static async Task Main(string[] args)
    {
        Assembly.LoadFrom(ProtectionsFile);
        var encryptionModuleDefMD = ModuleDefMD.Load(EncryptionFile);

        var container = new Application().BuildContainer();
        var logger = container.Resolve<ILogger>();
        var configuration = container.Resolve<IConfiguration>();

        var currentAssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        Directory.CreateDirectory("Output");
        var baseDirectory = Path.Combine(currentAssemblyDirectory, "Base");
        var outputDirectory = Path.Combine(currentAssemblyDirectory, "Output");
        var bitMonoContext = new BitMonoContext
        {
            BaseDirectory = baseDirectory,
            OutputDirectory = outputDirectory,
            Watermark = configuration.GetValue<bool>(nameof(BitMonoContext.Watermark)),
        };

        string moduleFile = null;
        if (args.Length != 0)
        {
            moduleFile = args[0];
        }
        else
        {
            var baseDirectoryFiles = Directory.GetFiles(bitMonoContext.BaseDirectory);
            if (baseDirectoryFiles.Length == 0)
            {
                logger.Warn("Please specify module file (target), " +
                    "drag and drop it to CLI or drop it in 'Base' directory!");
                Console.ReadLine();
                return;
            }

            moduleFile = baseDirectoryFiles[0]; 
        }

        bitMonoContext.ModuleFile = moduleFile;

        var moduleDefMD = ModuleDefMD.Load(moduleFile, new ModuleCreationOptions(CLRRuntimeReaderKind.Mono));
        var moduleWriterOptions = new ModuleWriterOptions(moduleDefMD);
        moduleWriterOptions.MetadataLogger = DummyLogger.NoThrowInstance;
        moduleWriterOptions.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack | MetadataFlags.PreserveAll;
        moduleWriterOptions.Cor20HeaderOptions.Flags = ComImageFlags.ILOnly;

        Assembly moduleAssembly = null;
        try
        {
            moduleAssembly = Assembly.LoadFrom(moduleFile);
        }
        catch (FileLoadException ex)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                logger.Warn($"Seems your Module - {moduleFile}, is blocked, try to unblock it (disabling the security) by opening propetries of the file, should be checked 'Unblock' & then pressed button 'Apply'");
            }
            else
            {
                logger.Warn($"Failed to load module assembly [{moduleFile}]! " + ex.ToString());
            }
        }

        if (moduleAssembly == null)
        {
            logger.Warn($"Failed to load module assembly [{moduleFile}]!");
            Console.ReadLine();
            return;
        }

        var protectionContext = new ProtectionContext
        {
            ModuleDefMD = moduleDefMD,
            ModuleWriterOptions = moduleWriterOptions,
            EncryptionModuleDefMD = encryptionModuleDefMD,
            TargetAssembly = Assembly.LoadFrom(moduleFile),
            BitMonoContext = bitMonoContext,
        };

        logger.Info("Loaded Module: " + moduleDefMD.Name);
        logger.Info("Protecting: " + bitMonoContext.ModuleFile);

        var protections = container.Resolve<ICollection<IProtection>>();
        protections = new DependencyResolver(protections, container.Resolve<IConfiguration>(), logger)
            .Sort(out ICollection<string> skipped);

        if (skipped.Any())
        {
            logger.Warn("Skipping next protections: " + string.Join(", ", skipped.Select(p => p ?? "NULL")));
        }

        var protectionsWithConditions = protections.Where(p => p is ICallingCondition);
        foreach (var protection in protections.Except(protectionsWithConditions))
        {
            logger.Info($"[{protection.GetType().FullName}] -> Executing.. ");
            try
            {
                await protection.ExecuteAsync(protectionContext);
                logger.Info($"[{protection.GetType().FullName}] -> Executed! ");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        await container.DisposeAsync();

        var stringBuilder = new StringBuilder()
            .Append(Path.GetFileNameWithoutExtension(moduleFile));

        if (bitMonoContext.Watermark)
        {
            stringBuilder.
                Append("_bitmono");
        }

        stringBuilder.Append(Path.GetExtension(moduleFile));

        var outputFile = Path.Combine(bitMonoContext.OutputDirectory, stringBuilder.ToString());
        bitMonoContext.ProtectedModuleFile = outputFile;

        try
        {
            using (moduleDefMD)
            using (var fileStream = File.Create(outputFile))
            {
                moduleDefMD.Write(fileStream, moduleWriterOptions);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex);
        }

        try
        {
            foreach (var protection in protectionsWithConditions)
            {
                if (protection is ICallingCondition callingCondition)
                {
                    if (callingCondition.Condition == CallingConditions.End)
                    {
                        logger.Info($"[{protection.GetType().FullName}] -> Executing.. ");
                        await protection.ExecuteAsync(protectionContext);
                        logger.Info($"[{protection.GetType().FullName}] -> Executed! ");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex);
        }

        logger.Info($"Saved protected module in {bitMonoContext.OutputDirectory} ...");
        Process.Start(bitMonoContext.OutputDirectory);
        logger.Info($"Completed!");

        var tips = configuration.GetSection("Tips").Get<string[]>();
        Random random = new Random();
        var tip = tips.Reverse().ToArray()[random.Next(0, tips.Length)];
        logger.Info("Today is your day! Generating helpful tip for you - see it a bit down!");
        logger.Info(tip);

        Console.ReadLine();
    }
}