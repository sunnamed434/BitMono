using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using BitMono.API.Ioc;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace BitMono.Host
{
    public class BitMonoApplication : IApplication
    {
        private readonly ContainerBuilder m_ContainerBuilder;
        private readonly IList<IModule> m_Modules;

        public BitMonoApplication()
        {
            m_ContainerBuilder = new ContainerBuilder();
            m_Modules = new List<IModule>();
        }

        public IApplication Populate(ICollection<ServiceDescriptor> descriptors)
        {
            m_ContainerBuilder.Populate(descriptors);
            return this;
        }
        public IApplication RegisterModule(IModule module)
        {
            m_Modules.Add(module);
            return this;
        }
        public AutofacServiceProvider Build()
        {
            foreach (var module in m_Modules)
            {
                m_ContainerBuilder.RegisterModule(module);
            }
            var container = m_ContainerBuilder.Build();
            return new AutofacServiceProvider(container.Resolve<ILifetimeScope>());
        }
    }
}