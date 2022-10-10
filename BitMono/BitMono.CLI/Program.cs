using Autofac;
using BitMono.API.Protecting;
using BitMono.Core.Configuration.Dependencies;
using BitMono.Core.Configuration.Extensions;
using BitMono.Core.Models;
using BitMono.Core.Protecting;
using BitMono.Host;
using BitMono.Host.Modules;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

public class Program
{
    const string EncryptionFile = nameof(BitMono) + "." + nameof(BitMono.Encryption) + ".dll";
    const string ProtectionsFile = nameof(BitMono) + "." + nameof(BitMono.Protections) + ".dll";

    private static async Task Main(string[] args)
    {
        Assembly.LoadFrom(ProtectionsFile);
        var encryptionModuleDefMD = ModuleDefMD.Load(EncryptionFile);

        var container = new BitMonoApplication().RegisterModule(new BitMonoModule(configureLogger =>
        {
            configureLogger.WriteTo.Async(configureSinkConfiguration =>
            {
                configureSinkConfiguration.Console();
            });
        })).Build();

        var logger = container.LifetimeScope.Resolve<ILogger>();
        var configuration = container.LifetimeScope.Resolve<IConfiguration>();

        var currentAssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        Directory.CreateDirectory("Base");
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
                logger.Warning("Please specify module file, drag and drop it to CLI or drop it in {0} directory!", bitMonoContext.BaseDirectory);
                Console.ReadLine();
                return;
            }

            moduleFile = baseDirectoryFiles[0]; 
        }

        bitMonoContext.ModuleFile = moduleFile;

        var assemblyResolver = new AssemblyResolver();
        var moduleContext = new ModuleContext(assemblyResolver);
        assemblyResolver.DefaultModuleContext = moduleContext;
        var moduleCreationOptions = new ModuleCreationOptions(assemblyResolver.DefaultModuleContext, CLRRuntimeReaderKind.Mono);
        var moduleDefMD = ModuleDefMD.Load(moduleFile, moduleCreationOptions);
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
                logger.Fatal("Seems Module {0}, is blocked, try to unblock it (disabling the security) by opening propetries of the file, should be checked 'Unblock' & then pressed button 'Apply'", moduleFile);
            }
            else
            {
                logger.Fatal(ex, "Failed to load module assembly!");
            }
            Console.ReadLine();
            return;
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, "Failed to load module assembly!");
            Console.ReadLine();
            return;
        }

        if (moduleAssembly == null)
        {
            logger.Fatal("Failed to load module assembly {0}!", moduleFile);
            Console.ReadLine();
            return;
        }

        var protectionContext = new ProtectionContext
        {
            ModuleDefMD = moduleDefMD,
            ModuleCreationOptions = moduleCreationOptions,
            ModuleWriterOptions = moduleWriterOptions,
            EncryptionModuleDefMD = encryptionModuleDefMD,
            Assembly = Assembly.LoadFrom(moduleFile),
            BitMonoContext = bitMonoContext,
        };

        logger.Information("Loaded Module {0}", moduleDefMD.Name);
        logger.Warning("Resolving dependecies {0}", bitMonoContext.ModuleFile);

        var protections = container.LifetimeScope.Resolve<ICollection<IProtection>>();
        protections = new DependencyResolver(protections, configuration.GetProtectionSettings(), logger)
            .Sort(out ICollection<string> skipped);
        var protectionsWithConditions = protections.Where(p => p is ICallingCondition);
        protections.Except(protectionsWithConditions);

        var bitMonoAssemblyResolver = new BitMonoAssemblyResolver(protectionContext, logger);
        await bitMonoAssemblyResolver.ResolveAsync();
        if (configuration.GetValue<bool>("FailOnNoRequiredDependency"))
        {
            if (bitMonoAssemblyResolver.ResolvingFailed)
            {
                logger.Warning("Drop dependencies in {0}, or set in config FailOnNoRequiredDependency to false", bitMonoContext.BaseDirectory);
                Console.ReadLine();
                return;
            }
        }
        logger.Information("Protecting: {0}", bitMonoContext.ModuleFile);

        if (skipped.Any())
        {
            logger.Warning("Skip protections: {0}", string.Join(", ", skipped.Select(p => p ?? "NULL")));
        }
        
        if (protections.Any())
        {
            logger.Information("Execute protections: {0}", string.Join(", ", protections.Select(p => p.GetType().Name ?? "NULL")));
        }

        if (protectionsWithConditions.Any())
        {
            logger.Information("Execute calling condition protections: {0}", string.Join(", ", protectionsWithConditions.Select(p => p.GetType().Name ?? "NULL")));
        }

        foreach (var protection in protections.Except(protectionsWithConditions))
        {
            try
            {
                logger.Information("{0} -> Executing.. ", protection.GetType().FullName);
                await protection.ExecuteAsync(protectionContext);
                logger.Information("{0} -> Executed! ", protection.GetType().FullName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error while executing protections!");
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
            logger.Error(ex, "Error while saving module!");
        }

        try
        {
            foreach (var protection in protectionsWithConditions)
            {
                if (protection is ICallingCondition callingCondition)
                {
                    if (callingCondition.Condition == CallingConditions.End)
                    {
                        logger.Information("{0} -> Executing.. ", protection.GetType().FullName);
                        await protection.ExecuteAsync(protectionContext);
                        logger.Information("{0} -> Executed! ", protection.GetType().FullName);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error while executing Calling condition protections!");
        }

        logger.Information("Saved protected module in {0}", bitMonoContext.OutputDirectory);
        logger.Information("Completed!");
        Process.Start(bitMonoContext.OutputDirectory);

        var tips = configuration.GetSection("Tips").Get<string[]>();
        Random random = new Random();
        var tip = tips.Reverse().ToArray()[random.Next(0, tips.Length)];
        logger.Information("Today is your day! Generating helpful tip for you - see it a bit down!");
        logger.Information(tip);
        Console.ReadLine();
    }
}