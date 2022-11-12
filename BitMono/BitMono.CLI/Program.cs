using Autofac;
using BitMono.API.Configuration;
using BitMono.API.Protecting;
using BitMono.CLI.Modules;
using BitMono.Core.Configuration.Extensions;
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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

public class Program
{
    const string Protections = nameof(BitMono) + "." + nameof(BitMono.Protections) + ".dll";
    const string ExternalComponents = nameof(BitMono) + "." + nameof(BitMono.ExternalComponents) + ".dll";

    static CancellationTokenSource CancellationToken = new CancellationTokenSource();

    private static async Task Main(string[] args)
    {
        var serviceProvider = new BitMonoApplication().RegisterModule(new BitMonoModule(configureLogger =>
        {
            configureLogger.WriteTo.Async(configure => configure.Console(
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}"));
        })).Build();

        var moduleFileName = await new CLIBitMonoModuleFileResolver(args).ResolveAsync();
        var logger = serviceProvider.LifetimeScope.Resolve<ILogger>().ForContext<Program>();
        if (string.IsNullOrWhiteSpace(moduleFileName))
        {
            logger.Fatal("Please, specify file, drag-and-drop it in BitMono CLI");
            Console.ReadLine();
            return;
        }

        var domainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var protectionsFile = Path.Combine(domainBaseDirectory, Protections);
        var externalComponentsFile = Path.Combine(domainBaseDirectory, ExternalComponents);
        var externalComponentsModuleDefMD = ModuleDefMD.Load(externalComponentsFile);

        Assembly.LoadFrom(protectionsFile);

        var libsDirectoryName = Path.Combine(domainBaseDirectory, "libs");
        var outputDirectoryName = Path.Combine(Path.GetDirectoryName(args[0]), "output");
        Directory.CreateDirectory(libsDirectoryName);
        Directory.CreateDirectory(outputDirectoryName);

        var obfuscationConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoObfuscationConfiguration>();
        var protectionsConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoProtectionsConfiguration>();
        var appSettingsConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoAppSettingsConfiguration>();
        var bitMonoContext = await new BitMonoContextCreator(obfuscationConfiguration).CreateAsync(outputDirectoryName, libsDirectoryName);
        bitMonoContext.ModuleFileName = moduleFileName;

        var protections = serviceProvider.LifetimeScope.Resolve<ICollection<IProtection>>();
        var protectionSettings = protectionsConfiguration.GetProtectionSettings();
        var moduleFileBytes = File.ReadAllBytes(bitMonoContext.ModuleFileName);
        await new BitMonoObfuscator(serviceProvider, new CLIModuleDefMDWriter(), new ModuleDefMDCreator(moduleFileBytes), logger)
            .ObfuscateAsync(bitMonoContext, externalComponentsModuleDefMD, protections, protectionSettings, CancellationToken.Token);

        if (obfuscationConfiguration.Configuration.GetValue<bool>(nameof(Obfuscation.OpenFileDestinationInFileExplorer)))
        {
            Process.Start(bitMonoContext.OutputPath);
        }

        await new TipsNotifier(appSettingsConfiguration, logger).NotifyAsync();

        await serviceProvider.DisposeAsync();
        Console.ReadLine();
    }
}