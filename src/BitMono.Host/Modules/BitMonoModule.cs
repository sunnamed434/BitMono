namespace BitMono.Host.Modules;

public class BitMonoModule : Module
{
    private const string DateVariableName = "date";
    private const string DateTimeFormat = "yyyy-MM-dd-HH-mm-ss";
    private readonly Action<ContainerBuilder>? _configureContainer;
    private readonly Action<ServiceCollection>? _configureServices;
    private readonly Action<LoggerConfiguration>? _configureLogger;
    private readonly string? _loggingFile;

    public BitMonoModule(
        Action<ContainerBuilder>? configureContainer = null,
        Action<ServiceCollection>? configureServices = null,
        Action<LoggerConfiguration>? configureLogger = null,
        string? loggingFile = null)
    {
        _configureContainer = configureContainer;
        _configureServices = configureServices;
        _configureLogger = configureLogger;
        _loggingFile = loggingFile;
    }

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    protected override void Load(ContainerBuilder containerBuilder)
    {
        _configureContainer?.Invoke(containerBuilder);

        var loggingConfigurationRoot = new ConfigurationBuilder().AddJsonFileEx(configure =>
        {
            configure.Path = _loggingFile ?? KnownConfigNames.Logging;
            configure.Optional = false;
            configure.Variables = new Dictionary<string, string>
            {
                {
                    DateVariableName,
                    DateTime.Now.ToString(DateTimeFormat)
                }
            };
            configure.ResolveFileProvider();
        }).Build();

        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(loggingConfigurationRoot);

        _configureLogger?.Invoke(loggerConfiguration);

        var logger = loggerConfiguration.CreateLogger();
        containerBuilder.Register<ILogger>(_ => logger);

        var serviceCollection = new ServiceCollection();
        _configureServices?.Invoke(serviceCollection);

        containerBuilder.RegisterType<EngineContextAccessor>()
            .As<IEngineContextAccessor>()
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterType<ProtectionContextFactory>()
            .AsSelf()
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterType<ProtectionParametersFactory>()
            .AsSelf()
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.Register<RandomNext>(_ => RandomService.RandomNext)
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterType<Renamer>()
            .OwnedByLifetimeScope()
            .SingleInstance();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        containerBuilder.RegisterAssemblyTypes(assemblies)
            .PublicOnly()
            .Where(t => t.GetInterface(nameof(IMemberResolver)) != null)
            .AsImplementedInterfaces()
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterAssemblyTypes(assemblies)
            .PublicOnly()
            .AsClosedTypesOf(typeof(ICriticalAnalyzer<>))
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterAssemblyTypes(assemblies)
            .PublicOnly()
            .AsClosedTypesOf(typeof(IAttributeResolver<>))
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.Populate(serviceCollection);
    }
}