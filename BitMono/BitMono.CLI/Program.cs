using Autofac;
using BitMono.API.Configuration;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Pipeline;
using BitMono.API.Protecting.Resolvers;
using BitMono.CLI.Writers;
using BitMono.Core.Configuration.Extensions;
using BitMono.Core.Protecting.Modules;
using BitMono.Core.Protecting.Resolvers;
using BitMono.Host;
using BitMono.Host.Modules;
using BitMono.Shared.Models;
using dnlib.DotNet;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

public class Program
{
    const string ProtectionsFile = nameof(BitMono) + "." + nameof(BitMono.Protections) + ".dll";
    const string ExternalComponentsFile = nameof(BitMono) + "." + nameof(BitMono.ExternalComponents) + ".dll";

    static CancellationTokenSource CancellationToken = new CancellationTokenSource();

    private static async Task Main(string[] args)
    {
        Assembly.LoadFrom(ProtectionsFile);
        var externalComponentsModuleDefMD = ModuleDefMD.Load(ExternalComponentsFile);

        var serviceProvider = new BitMonoApplication().RegisterModule(new BitMonoModule(configureLogger =>
        {
            configureLogger.WriteTo.Async(configure => configure.Console(
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}"));
        })).Build();

        var logger = serviceProvider.LifetimeScope.Resolve<ILogger>().ForContext<Program>();
        var obfuscationConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoObfuscationConfiguration>().Configuration;
        var protectionsConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoProtectionsConfiguration>().Configuration;
        var appSettingsConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoAppSettingsConfiguration>().Configuration;
        var obfuscationAttributeExcludingResolver = serviceProvider.LifetimeScope.Resolve<IObfuscationAttributeExcludingResolver>();

        var currentAssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        Directory.CreateDirectory("base");
        Directory.CreateDirectory("output");
        var baseDirectory = Path.Combine(currentAssemblyDirectory, "base");
        var outputDirectory = Path.Combine(currentAssemblyDirectory, "output");
        var bitMonoContext = new BitMonoContext
        {
            BaseDirectory = baseDirectory,
            OutputDirectory = outputDirectory,
            Watermark = obfuscationConfiguration.GetValue<bool>(nameof(Obfuscation.Watermark)),
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

        var moduleDefMDCreationResult = await new ModuleDefMDCreator().CreateAsync(moduleFile);
        var protectionContext = new ProtectionContext
        {
            ModuleDefMD = moduleDefMDCreationResult.ModuleDefMD,
            ModuleCreationOptions = moduleDefMDCreationResult.ModuleCreationOptions,
            ModuleWriterOptions = moduleDefMDCreationResult.ModuleWriterOptions,
            ExternalComponentsModuleDefMD = externalComponentsModuleDefMD,
            Importer = new Importer(moduleDefMDCreationResult.ModuleDefMD),
            ExternalComponentsImporter = new Importer(externalComponentsModuleDefMD, ImporterOptions.TryToUseMethodDefs),
            BitMonoContext = bitMonoContext,
        };

        logger.Information("Loaded Module {0}", moduleDefMDCreationResult.ModuleDefMD.Name);
        logger.Warning("Resolving dependecies {0}", bitMonoContext.ModuleFile);

        var protections = serviceProvider.LifetimeScope.Resolve<ICollection<IProtection>>();
        protections = new DependencyResolver(protections, protectionsConfiguration.GetProtectionSettings(), logger)
            .Sort(out ICollection<string> skipped);
        var stageProtections = protections.Where(p => p is IStageProtection).Cast<IStageProtection>();
        var pipelineProtections = protections.Where(p => p is IPipelineProtection).Cast<IPipelineProtection>();
        var obfuscationAttributeExcludingProtections = protections.Where(p =>
            obfuscationAttributeExcludingResolver.TryResolve(moduleDefMDCreationResult.ModuleDefMD.Assembly, p.GetType().Name, 
            out ObfuscationAttribute obfuscationAttribute) && obfuscationAttribute.Exclude);

        protections.Except(stageProtections).Except(obfuscationAttributeExcludingProtections);

        if (obfuscationConfiguration.GetValue<bool>(nameof(Obfuscation.FailOnNoRequiredDependency)))
        {
            var resolvingSucceed = await new BitMonoAssemblyResolver(protectionContext, logger).ResolveAsync();
            if (resolvingSucceed == false)
            {
                logger.Warning("Drop dependencies in {0}, or set in config FailOnNoRequiredDependency to false", bitMonoContext.BaseDirectory);
                Console.ReadLine();
                return;
            }
        }

        if (skipped.Any())
        {
            logger.Warning("Skip protections: {0}", string.Join(", ", skipped.Select(p => p ?? "NULL")));
        }

        if (obfuscationAttributeExcludingProtections.Any())
        {
            logger.Warning("Skip protections with obfuscation attribute excluding: {0}", string.Join(", ", obfuscationAttributeExcludingProtections.Select(p => p.GetType().Name ?? "NULL")));
        }

        if (protections.Any())
        {
            logger.Information("Execute protections: {0}", string.Join(", ", protections.Select(p => p.GetType().Name ?? "NULL")));
        }

        if (stageProtections.Any())
        {
            logger.Information("Execute calling condition protections: {0}", string.Join(", ", stageProtections.Select(p => p.GetType().Name ?? "NULL")));
        }

        var stringBuilder = new StringBuilder()
            .Append(Path.GetFileNameWithoutExtension(moduleFile));

        if (bitMonoContext.Watermark)
        {
            stringBuilder.
                Append("_bitmono");
        }

        stringBuilder.Append(Path.GetExtension(moduleFile));

        var outputFile = Path.Combine(bitMonoContext.OutputDirectory, stringBuilder.ToString());
        bitMonoContext.OutputModuleFile = outputFile;

        logger.Information("Preparing to protect: {0}", bitMonoContext.ModuleFile);
        var bitMonoEngine = new BitMonoEngine(protections, protectionContext, new CLIModuleDefMDWriter(protectionContext), logger);
        await bitMonoEngine.StartAsync(CancellationToken.Token);

        logger.Information("Saved protected module in {0}", bitMonoContext.OutputDirectory);
        logger.Information("Completed!");
        if (obfuscationConfiguration.GetValue<bool>(nameof(Obfuscation.OpenFileDestinationInFileExplorer)))
        {
            Process.Start(bitMonoContext.OutputDirectory);
        }

        var tips = appSettingsConfiguration.GetSection("Tips").Get<string[]>();
        Random random = new Random();
        var tip = tips.Reverse().ToArray()[random.Next(0, tips.Length)];
        logger.Information("Today is your day! Generating helpful tip for you - see it a bit down!");
        logger.Information(tip);

        await serviceProvider.DisposeAsync();
        Console.ReadLine();
    }
}