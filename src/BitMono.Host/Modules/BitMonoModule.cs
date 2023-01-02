namespace BitMono.Host.Modules;

public class BitMonoModule : Module
{
    private readonly Action<LoggerConfiguration> m_ConfigureLogger;
    private readonly Action<ConfigurationBuilder> m_ConfigureConfigurationBuilder;

    public BitMonoModule(
        Action<LoggerConfiguration> configureLogger = default,
        Action<ConfigurationBuilder> configureConfigurationBuilder = default)
    {
        m_ConfigureLogger = configureLogger;
        m_ConfigureConfigurationBuilder = configureConfigurationBuilder;
    }

    protected override void Load(ContainerBuilder containerBuilder)
    {
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
            containerBuilder.Register<ILogger>((_, _) =>
            {
                return logger;
            });
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

        containerBuilder.Register(context => new BitMonoAppSettingsConfiguration())
            .As<IBitMonoAppSettingsConfiguration>()
            .OwnedByLifetimeScope();

        containerBuilder.Register(context => new BitMonoCriticalsConfiguration())
            .As<IBitMonoCriticalsConfiguration>()
            .OwnedByLifetimeScope();

        containerBuilder.Register(context => new BitMonoProtectionsConfiguration())
            .As<IBitMonoProtectionsConfiguration>()
            .OwnedByLifetimeScope();

        containerBuilder.Register(context => new BitMonoObfuscationConfiguration())
            .As<IBitMonoObfuscationConfiguration>()
            .OwnedByLifetimeScope();

        containerBuilder.RegisterType<Renamer>()
            .As<IRenamer>()
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterType<Injector>()
            .As<IInjector>()
            .OwnedByLifetimeScope()
            .SingleInstance();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        containerBuilder.RegisterAssemblyTypes(assemblies)
            .PublicOnly()
            .Where(t => t.GetInterface(nameof(ICustomAttributeResolver)) != null)
            .AsImplementedInterfaces()
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterAssemblyTypes(assemblies)
            .PublicOnly()
            .Where(t => t.GetInterface(nameof(IAttemptAttributeResolver)) != null)
            .AsImplementedInterfaces()
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterAssemblyTypes(assemblies)
            .PublicOnly()
            .Where(t => t.GetInterface(nameof(IAttributeResolver)) != null)
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterAssemblyTypes(assemblies)
            .PublicOnly()
            .AsClosedTypesOf(typeof(ICriticalAnalyzer<>))
            .OwnedByLifetimeScope()
            .SingleInstance();

        containerBuilder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
            .PublicOnly()
            .Where(t => t.GetInterface(nameof(IMemberResolver)) != null)
            .OwnedByLifetimeScope()
            .AsImplementedInterfaces()
            .SingleInstance();

        containerBuilder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
            .PublicOnly()
            .Where(t => 
                t.GetInterface(nameof(IProtection)) != null 
                && t.GetInterface(nameof(IPhaseProtection)) == null)
            .OwnedByLifetimeScope()
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}