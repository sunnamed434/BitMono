using Autofac;
using BitMono.API.Configuration;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Resolvers;
using BitMono.CLI.Modules;
using BitMono.Core.Extensions.Configuration;
using BitMono.Host;
using BitMono.Host.Modules;
using BitMono.Obfuscation;
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
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

public class Program
{
    const string Protections = nameof(BitMono) + "." + nameof(BitMono.Protections) + ".dll";

    static CancellationTokenSource CancellationToken = new CancellationTokenSource();

    private static async Task Main(string[] args)
    {
        try
        {
            var moduleFileName = await new CLIBitMonoModuleFileResolver(args).ResolveAsync();
            if (string.IsNullOrWhiteSpace(moduleFileName))
            {
                Console.WriteLine("File not specified, please specify path to the file here: ");
                moduleFileName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(moduleFileName))
                {
                    Console.WriteLine("Please, specify file or drag-and-drop in BitMono CLI");
                    Console.ReadLine();
                    return;
                }
            }

            var domainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var protectionsFile = Path.Combine(domainBaseDirectory, Protections);
            var externalComponentsModuleDefMD = ModuleDefMD.Load(typeof(BitMono.ExternalComponents.Hooking).Module);
            Assembly.LoadFrom(protectionsFile);

            var moduleFileBaseDirectory = Path.GetDirectoryName(moduleFileName);
            var dependenciesDirectoryName = Path.Combine(moduleFileBaseDirectory, "libs");
            var outputDirectoryName = Path.Combine(moduleFileBaseDirectory, "output");
            Directory.CreateDirectory(dependenciesDirectoryName);
            Directory.CreateDirectory(outputDirectoryName);

            var serviceProvider = new BitMonoApplication().RegisterModule(new BitMonoModule(configureLogger =>
            {
                configureLogger.WriteTo.Async(configure => configure.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}"));
            })).Build();

            var obfuscationConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoObfuscationConfiguration>();
            var protectionsConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoProtectionsConfiguration>();
            var appSettingsConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoAppSettingsConfiguration>();
            var dnlibDefFeatureObfuscationAttributeHavingResolver = serviceProvider.LifetimeScope.Resolve<IDnlibDefFeatureObfuscationAttributeHavingResolver>();

            var bitMonoContext = new BitMonoContextCreator(new DependenciesDataResolver(dependenciesDirectoryName), obfuscationConfiguration).Create(outputDirectoryName);
            bitMonoContext.ModuleFileName = moduleFileName;

            var protections = serviceProvider.LifetimeScope.Resolve<ICollection<IProtection>>().ToList();
            var moduleFileBytes = File.ReadAllBytes(bitMonoContext.ModuleFileName);
            var logger = serviceProvider.LifetimeScope.Resolve<ILogger>().ForContext<Program>();
            var protectionSettings = protectionsConfiguration.GetProtectionSettings();
            await new BitMonoEngine(
                new CLIModuleDefMDWriter(),
                new ModuleDefMDCreator(moduleFileBytes),
                dnlibDefFeatureObfuscationAttributeHavingResolver,
                obfuscationConfiguration,
                logger)
                .ObfuscateAsync(bitMonoContext, externalComponentsModuleDefMD, protections, protectionSettings, CancellationToken.Token);

            if (obfuscationConfiguration.Configuration.GetValue<bool>(nameof(Obfuscation.OpenFileDestinationInFileExplorer)))
            {
                Process.Start(bitMonoContext.OutputPath);
            }

            await new TipsNotifier(appSettingsConfiguration, logger).NotifyAsync();

            await serviceProvider.DisposeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something went wrong! " + ex.ToString());
        }
        Console.ReadLine();
    }
}