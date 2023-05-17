namespace BitMono.Host.Modules;

public class BitMonoModule : Module
{
    private readonly Action<ContainerBuilder>? m_ConfigureContainer;
    private readonly Action<ServiceCollection>? m_ConfigureServices;
    private readonly Action<LoggerConfiguration>? m_ConfigureLogger;
    private const string LoggingFileName = "logging.json";
    private const string DateVariableName = "date";
    private const string DateTimeFormat = "yyyy-MM-dd-HH-mm-ss";

    public BitMonoModule(
        Action<ContainerBuilder>? configureContainer = null,
        Action<ServiceCollection>? configureServices = null,
        Action<LoggerConfiguration>? configureLogger = null)
    {
        m_ConfigureContainer = configureContainer;
        m_ConfigureServices = configureServices;
        m_ConfigureLogger = configureLogger;
    }

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    protected override void Load(ContainerBuilder containerBuilder)
    {
        m_ConfigureContainer?.Invoke(containerBuilder);

        var loggingConfigurationRoot = new ConfigurationBuilder().AddJsonFileEx(configure =>
        {
            configure.Path = LoggingFileName;
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

        m_ConfigureLogger?.Invoke(loggerConfiguration);

        var logger = loggerConfiguration.CreateLogger();
        containerBuilder.Register<ILogger>(_ => logger);

        var serviceCollection = new ServiceCollection();
        m_ConfigureServices?.Invoke(serviceCollection);

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