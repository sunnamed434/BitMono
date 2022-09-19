using Autofac;
using BitMono.API.Injection;
using BitMono.API.Injection.Fields;
using BitMono.API.Injection.Methods;
using BitMono.API.Injection.Types;
using BitMono.API.Ioc;
using BitMono.API.Naming;
using BitMono.API.Protections;
using BitMono.Core.Analyzing;
using BitMono.Core.Attributes;
using BitMono.Core.Injection;
using BitMono.Core.Injection.Fields;
using BitMono.Core.Injection.Methods;
using BitMono.Core.Injection.Types;
using BitMono.Core.Naming;
using System.Linq;
using System.Reflection;

namespace BitMono
{
    public class Application : IApplication
    {
        public IContainer BuildContainer()
        {
            var containerBuilder = new ContainerBuilder();

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

            containerBuilder.Register(d => new TypeDefCriticalAnalyzer());
            containerBuilder.Register(d => new FieldDefCriticalAnalyzerr());
            containerBuilder.Register(d => new EventDefCriticalAnalyzer());
            containerBuilder.Register(d => new MethodDefCriticalAnalyzer());

            containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .PublicOnly()
                .Where(t => t.GetCustomAttribute<ExceptRegisterServiceAttribute>() == null
                            && t.GetInterface(nameof(IProtection)) != null)
                .OwnedByLifetimeScope()
                .AsImplementedInterfaces()
                .SingleInstance();

            return containerBuilder.Build();
        }
    }
}