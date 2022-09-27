using Autofac;
using BitMono.API.Ioc;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Analyzing;
using BitMono.API.Protecting.Injection;
using BitMono.API.Protecting.Injection.Fields;
using BitMono.API.Protecting.Injection.Methods;
using BitMono.API.Protecting.Injection.Types;
using BitMono.API.Protecting.Renaming;
using BitMono.Core.Attributes;
using BitMono.Core.Injection;
using BitMono.Core.Protecting.Injection.Fields;
using BitMono.Core.Protecting.Injection.Methods;
using BitMono.Core.Protecting.Injection.Types;
using BitMono.Core.Protecting.Renaming;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Reflection;

namespace BitMono.Host
{
    public class Application : IApplication
    {
        public IContainer BuildContainer()
        {
            var containerBuilder = new ContainerBuilder();

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("config.json", optional: true, reloadOnChange: true);
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
                .Where(t => t.GetCustomAttribute<ExceptRegisterProtectionAttribute>(false) == null
                            && t.GetInterface(nameof(IProtection)) != null)
                .OwnedByLifetimeScope()
                .AsImplementedInterfaces()
                .SingleInstance();

            containerBuilder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
                .PublicOnly()
                .Where(t => t.GetCustomAttribute<ServiceImplementationAttribute>(false) != null)
                .OwnedByLifetimeScope()
                .AsImplementedInterfaces()
                .SingleInstance();

            return containerBuilder.Build();
        }
    }
}