using Autofac;
using BitMono.API.Injection;
using BitMono.API.Injection.Fields;
using BitMono.API.Injection.Methods;
using BitMono.API.Injection.Types;
using BitMono.API.Ioc;
using BitMono.API.Naming;
using BitMono.API.Protections;
using BitMono.Core.Attributes;
using BitMono.Core.Injection;
using BitMono.Core.Injection.Fields;
using BitMono.Core.Injection.Methods;
using BitMono.Core.Injection.Types;
using BitMono.Core.Naming;
using BitMono.Core.Protections;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System.Linq;
using System.Reflection;

namespace BitMono
{
    public class Application : IApplication
    {
        private readonly ModuleDefMD m_ModuleDefMD;
        private readonly ModuleWriterOptions m_ModuleWriterOptions;
        private readonly ModuleDefMD m_EncryptionModuleDefMD;
        private readonly Assembly m_TargetAssembly;

        public Application(ModuleDefMD moduleDefMD, ModuleWriterOptions moduleWriterOptions, ModuleDefMD encryptionModuleDefMD, Assembly targetAssembly)
        {
            m_ModuleDefMD = moduleDefMD;
            m_ModuleWriterOptions = moduleWriterOptions;
            m_EncryptionModuleDefMD = encryptionModuleDefMD;
            m_TargetAssembly = targetAssembly;
        }


        public IContainer BuildContainer()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.Register(p => new ProtectionContext(m_ModuleDefMD, m_ModuleWriterOptions, m_EncryptionModuleDefMD, m_TargetAssembly))
                .SingleInstance();

            containerBuilder.RegisterType<Renamer>()
                .As<IRenamer>()
                .SingleInstance();

            containerBuilder.RegisterType<TypeSearcher>()
                .As<ITypeSearcher>()
                .SingleInstance();

            containerBuilder.RegisterType<TypeRemover>()
                .As<ITypeRemover>()
                .SingleInstance();

            containerBuilder.RegisterType<MethodSearcher>()
                .As<IMethodSearcher>()
                .SingleInstance();

            containerBuilder.RegisterType<MethodRemover>()
                .As<IMethodRemover>()
                .SingleInstance();

            containerBuilder.RegisterType<FieldSearcher>()
                .As<IFieldSearcher>()
                .SingleInstance();

            containerBuilder.RegisterType<FieldRemover>()
                .As<IFieldRemover>()
                .SingleInstance();

            containerBuilder.RegisterType<Injector>()
                .As<IInjector>()
                .SingleInstance();

            containerBuilder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .PublicOnly()
                .Where(t => t.GetCustomAttribute<ExceptRegisterServiceAttribute>() == null
                            && t.GetInterface(nameof(IProtection)) != null)
                .AsImplementedInterfaces()
                .SingleInstance();

            return containerBuilder.Build();
        }
    }
}