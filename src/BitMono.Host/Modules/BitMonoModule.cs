namespace BitMono.Host.Modules;

public class BitMonoModule : Module
{
    private readonly Action<ContainerBuilder> m_ConfigureServices;
    private readonly Action<LoggerConfiguration> m_ConfigureLogger;
    private readonly Action<ConfigurationBuilder> m_ConfigureConfigurationBuilder;

    public BitMonoModule(
        Action<ContainerBuilder> configureServices = default,
        Action<LoggerConfiguration> configureLogger = default,
        Action<ConfigurationBuilder> configureConfigurationBuilder = default)
    {
        m_ConfigureServices = configureServices;
        m_ConfigureLogger = configureLogger;
        m_ConfigureConfigurationBuilder = configureConfigurationBuilder;
    }

    protected override void Load(ContainerBuilder containerBuilder)
    {
        m_ConfigureServices?.Invoke(containerBuilder);

        var serviceCollection = new ServiceCollection();
        
        var currentAssemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        var fileFormat = string.Format(Path.Combine(currentAssemblyDirectory, "logs", "bitmono-{0:yyyy-MM-dd-HH-mm-ss}.log"), DateTime.Now).Replace("\\", "/");

        var loggerConfiguration = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Async(configure =>
            {
                configure.File(fileFormat, rollingInterval: RollingInterval.Infinite, restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}");
            });

        if (m_ConfigureLogger != null)
        {
            m_ConfigureLogger.Invoke(loggerConfiguration);

            var logger = loggerConfiguration.CreateLogger();
            containerBuilder.Register<ILogger>((_, _) => logger);
        }

        var configurationBuilder = new ConfigurationBuilder();
        if (m_ConfigureConfigurationBuilder != null)
        {
            m_ConfigureConfigurationBuilder.Invoke(configurationBuilder);
            var configuration = configurationBuilder.Build();
            containerBuilder.Register(context => configuration)
                .As<IConfiguration>()
                .OwnedByLifetimeScope();
        }
        
        serviceCollection.AddOptions();
        var protections = new BitMonoProtectionsConfiguration();
        var criticals = new BitMonoCriticalsConfiguration();
        var obfuscation = new BitMonoObfuscationConfiguration();
        serviceCollection.Configure<ProtectionSettings>(options => protections.Configuration.Bind(options));
        serviceCollection.Configure<Criticals>(criticals.Configuration);
        serviceCollection.Configure<Obfuscation>(obfuscation.Configuration);
        
        containerBuilder.Register(context => protections)
            .As<IBitMonoProtectionsConfiguration>()
            .OwnedByLifetimeScope();
        
        containerBuilder.Register(context => criticals)
            .As<IBitMonoCriticalsConfiguration>()
            .OwnedByLifetimeScope();

        containerBuilder.Register(context => obfuscation)
            .As<IBitMonoObfuscationConfiguration>()
            .OwnedByLifetimeScope();

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

        containerBuilder.RegisterAssemblyTypes(assemblies)
            .PublicOnly()
            .Where(t => t.GetInterface(nameof(IPhaseProtection)) == null && t.GetInterface(nameof(IProtection)) != null)
            .OwnedByLifetimeScope()
            .AsImplementedInterfaces()
            .SingleInstance();
        
        containerBuilder.Populate(serviceCollection);
    }
}