using Autofac;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using BitMono.API.Protecting.Injection;
using BitMono.API.Protecting.Injection.FieldDefs;
using BitMono.API.Protecting.Injection.MethodDefs;
using BitMono.API.Protecting.Injection.TypeDefs;
using BitMono.API.Protecting.Renaming;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Protecting.Injection;
using BitMono.Core.Protecting.Injection.FieldDefs;
using BitMono.Core.Protecting.Injection.MethodDefs;
using BitMono.Core.Protecting.Injection.TypeDefs;
using BitMono.Core.Protecting.Renaming;
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
        private readonly Action<ConfigurationBuilder> m_ConfigureConfiguration;

        public BitMonoModule(
            Action<LoggerConfiguration> configureLogger = default,
            Action<ConfigurationBuilder> configureConfiguration = default)
        {
            m_ConfigureLogger = configureLogger;
            m_ConfigureConfiguration = configureConfiguration;
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

            m_ConfigureLogger?.Invoke(loggerConfiguration);

            var logger = loggerConfiguration.CreateLogger();

            containerBuilder.Register<ILogger>((_, _) =>
            {
                return logger;
            });

            var configurationBuilder = new ConfigurationBuilder();
            m_ConfigureConfiguration?.Invoke(configurationBuilder);
            configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            configurationBuilder.AddJsonFile("criticals.json", optional: true, reloadOnChange: true);
            configurationBuilder.AddJsonFile("protections.json", optional: true, reloadOnChange: true);
            configurationBuilder.AddJsonFile("translations.json", optional: true, reloadOnChange: true);
            var configuration = configurationBuilder.Build();

            containerBuilder.Register(context => configuration)
                .As<IConfiguration>()
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
                    && t.GetInterface(nameof(IProtectionPhase)) == null)
                .OwnedByLifetimeScope()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}