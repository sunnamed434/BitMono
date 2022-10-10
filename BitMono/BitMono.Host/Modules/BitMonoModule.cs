using Autofac;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using BitMono.API.Protecting.Injection;
using BitMono.API.Protecting.Injection.Fields;
using BitMono.API.Protecting.Injection.Methods;
using BitMono.API.Protecting.Injection.Types;
using BitMono.API.Protecting.Renaming;
using BitMono.Core.Injection;
using BitMono.Core.Protecting.Injection.Fields;
using BitMono.Core.Protecting.Injection.Methods;
using BitMono.Core.Protecting.Injection.Types;
using BitMono.Core.Protecting.Renaming;
using Microsoft.Extensions.Configuration;
using Serilog;
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
            var file = Path.Combine(currentAssemblyDirectory, "logs", "bitMono-{HalfHour}.log");

            containerBuilder.Register<ILogger>((_, _) =>
            {
                var loggerConfiguration = new LoggerConfiguration();
                m_ConfigureLogger?.Invoke(loggerConfiguration);

                return loggerConfiguration
                    .WriteTo.Async((c) =>
                    {
                        c.RollingFile(file, shared: true);
                    })
                    .CreateLogger();
            });

            var configurationBuilder = new ConfigurationBuilder();
            m_ConfigureConfiguration?.Invoke(configurationBuilder);
            configurationBuilder.AddJsonFile("config.json", optional: true, reloadOnChange: true);
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

            containerBuilder.RegisterType<TypeSearcher>()
                .As<ITypeSearcher>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterType<TypeRemover>()
                .As<ITypeRemover>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterType<MethodSearcher>()
                .As<IMethodSearcher>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterType<MethodRemover>()
                .As<IMethodRemover>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterType<FieldSearcher>()
                .As<IFieldSearcher>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterType<FieldRemover>()
                .As<IFieldRemover>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterType<Injector>()
                .As<IInjector>()
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
                .PublicOnly()
                .AsClosedTypesOf(typeof(ICriticalAnalyzer<>))
                .OwnedByLifetimeScope()
                .SingleInstance();

            containerBuilder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
                .PublicOnly()
                .Where(t => t.GetInterface(nameof(IProtection)) != null)
                .OwnedByLifetimeScope()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}