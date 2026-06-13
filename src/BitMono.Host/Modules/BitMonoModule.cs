using BitMono.API;
using BitMono.API.Analyzing;
using BitMono.API.Resolvers;
using BitMono.Core.Factories;
using BitMono.Core.Renaming;
using BitMono.Core.Services;
using BitMono.Host.Ioc;
using BitMono.Shared.Configuration;
using BitMono.Shared.DependencyInjection;
using BitMono.Shared.Logging;
using BitMono.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using LoggerConfiguration = BitMono.Shared.Logging.LoggerConfiguration;
using ILogger = BitMono.Shared.Logging.ILogger;

namespace BitMono.Host.Modules;

public class BitMonoModule : IModule
{
    private const string DateTimeFormat = "yyyy-MM-dd-HH-mm-ss";

    private readonly Action<Container>? _configureContainer;
    private readonly Action<LoggerConfiguration>? _configureLogger;
    private readonly ObfuscationSettings? _obfuscationSettings;
    private readonly ProtectionSettings? _protectionSettings;
    private readonly string? _criticalsFile;
    private readonly string? _obfuscationFile;
    private readonly string? _protectionsFile;

    public BitMonoModule(
        Action<Container>? configureContainer = null,
        Action<LoggerConfiguration>? configureLogger = null,
        ObfuscationSettings? obfuscationSettings = null,
        ProtectionSettings? protectionSettings = null,
        string? criticalsFile = null,
        string? obfuscationFile = null,
        string? protectionsFile = null)
    {
        _configureContainer = configureContainer;
        _configureLogger = configureLogger;
        _obfuscationSettings = obfuscationSettings;
        _protectionSettings = protectionSettings;
        _criticalsFile = criticalsFile;
        _obfuscationFile = obfuscationFile;
        _protectionsFile = protectionsFile;
    }

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public void Load(Container container)
    {
        var loggerConfiguration = new LoggerConfiguration
        {
            WriteToConsole = true,
            WriteToFile = true,
            LogFilePath = Path.Combine("logs", $"bitmono-{DateTime.Now.ToString(DateTimeFormat)}.log"),
            MinimumLevel = LogLevel.Debug
        };
        _configureLogger?.Invoke(loggerConfiguration);

        var logger = new Logger(loggerConfiguration);
        container.Register<ILogger>(logger).AsSingleton();

        var criticalsSettings = SettingsLoader.Load<CriticalsSettings>(
            _criticalsFile ?? KnownConfigNames.Criticals);
        container.Register(criticalsSettings).AsSingleton();

        var obfuscationSettings = _obfuscationSettings ??
            SettingsLoader.Load<ObfuscationSettings>(_obfuscationFile ?? KnownConfigNames.Obfuscation);
        container.Register(obfuscationSettings).AsSingleton();

        var protectionSettings = _protectionSettings ??
            SettingsLoader.Load<ProtectionSettings>(_protectionsFile ?? KnownConfigNames.Protections);
        container.Register(protectionSettings).AsSingleton();

        container.Register<IEngineContextAccessor, EngineContextAccessor>().AsSingleton();
        container.Register<ProtectionContextFactory>().AsSingleton();
        container.Register<ProtectionParametersFactory>().AsSingleton();
        container.Register<RandomNext>(new RandomNext(RandomService.RandomNext)).AsSingleton();
        container.Register<Renamer>().AsSingleton();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        container.RegisterCollection<IMemberResolver>(assemblies);

        container.RegisterClosedTypesOf(assemblies, typeof(ICriticalAnalyzer<>));

        container.RegisterClosedTypesOf(assemblies, typeof(IAttributeResolver<>));

        // Call configureContainer AFTER all core dependencies are registered
        // This allows AddProtections to resolve protection dependencies correctly
        _configureContainer?.Invoke(container);
    }
}
