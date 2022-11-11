using Autofac;
using BitMono.API.Configuration;
using BitMono.CLI.Modules;
using BitMono.Host;
using BitMono.Host.Modules;
using BitMono.Obfuscation;
using BitMono.Shared.Models;
using dnlib.DotNet;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
        var obfuscationConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoObfuscationConfiguration>();
        var protectionsConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoProtectionsConfiguration>();
        var appSettingsConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoAppSettingsConfiguration>();

        var currentAssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        var bitMonoContext = await new BitMonoContextCreator(obfuscationConfiguration).CreateAsync(currentAssemblyDirectory, "base", "output");
        await new BitMonoObfuscator(serviceProvider, new CLIBitMonoModuleFileResolver(args), new CLIModuleDefMDWriter(), logger)
            .ObfuscateAsync(bitMonoContext, externalComponentsModuleDefMD, CancellationToken.Token);

        if (obfuscationConfiguration.Configuration.GetValue<bool>(nameof(Obfuscation.OpenFileDestinationInFileExplorer)))
        {
            Process.Start(bitMonoContext.OutputDirectory);
        }

        await new TipsNotifier(appSettingsConfiguration, logger).NotifyAsync();

        await serviceProvider.DisposeAsync();
        Console.ReadLine();
    }
}