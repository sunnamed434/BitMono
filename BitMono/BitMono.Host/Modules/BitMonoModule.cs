using Autofac;
using BitMono.API.Configuration;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using BitMono.API.Protecting.Injection;
using BitMono.API.Protecting.Injection.FieldDefs;
using BitMono.API.Protecting.Injection.MethodDefs;
using BitMono.API.Protecting.Injection.TypeDefs;
using BitMono.API.Protecting.Pipeline;
using BitMono.API.Protecting.Renaming;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting.Injection;
using BitMono.Core.Protecting.Injection.FieldDefs;
using BitMono.Core.Protecting.Injection.MethodDefs;
using BitMono.Core.Protecting.Injection.TypeDefs;
using BitMono.Core.Protecting.Renaming;
using BitMono.Core.Protecting.Resolvers;
using BitMono.Host.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Reflection;
using Module = Autofac.Module;

namespace BitMono.Host.Modules
{
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

            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
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

            containerBuilder.RegisterType<TypeDefSearcher>()
                .As<ITypeDefSearcher>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterType<TypeDefRemover>()
                .As<ITypeDefRemover>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterType<MethodDefsSearcher>()
                .As<IMethodDefSearcher>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterType<MethodDefsRemover>()
                .As<IMethodDefRemover>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterType<FieldDefSearcher>()
                .As<IFieldSearcher>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterType<FieldDefRemover>()
                .As<IFieldRemover>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterType<Injector>()
                .As<IInjector>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterType<DnlibDefFeatureObfuscationAttributeHavingResolver>()
                .As<IDnlibDefFeatureObfuscationAttributeHavingResolver>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            containerBuilder.RegisterAssemblyTypes(assemblies)
                .PublicOnly()
                .Where(t => t.GetInterface(nameof(ICustomAttributesResolver)) != null)
                .AsImplementedInterfaces()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterAssemblyTypes(assemblies)
                .PublicOnly()
                .Where(t => t.GetInterface(nameof(IObfuscationAttributeExcludingResolver)) != null)
                .AsImplementedInterfaces()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterAssemblyTypes(assemblies)
                .PublicOnly()
                .Where(t => t.GetInterface(nameof(IMethodImplAttributeExcludingResolver)) != null)
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
                .AsClosedTypesOf(typeof(ICriticalAnalyzer<>))
                .OwnedByLifetimeScope()
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
}