using BitMono.Host.Extensions;

namespace BitMono.Host.Modules;

public class BitMonoModule : Module
{
    private readonly Action<ContainerBuilder>? m_ConfigureContainer;
    private readonly Action<ServiceCollection>? m_ConfigureServices;
    private readonly Action<LoggerConfiguration>? m_ConfigureLogger;

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
            configure.Path = "logging.json";
            configure.Optional = false;
            configure.Variables = new Dictionary<string, string>
            {
                {
                    "date",
                    DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")
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

        containerBuilder.RegisterType<Renamer>()
            .As<IRenamer>()
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterType<MscorlibInjector>()
            .AsSelf()
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterType<CustomInjector>()
            .AsSelf()
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterType<RuntimeImplementations>()
            .AsSelf()
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterType<CustomAttributeResolver>()
            .AsSelf()
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterType<AttemptAttributeResolver>()
            .AsSelf()
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