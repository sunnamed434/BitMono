using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace BitMono.API.Ioc
{
    public interface IApplication
    {
        IApplication Populate(ICollection<ServiceDescriptor> descriptors);
        IApplication RegisterModule(IModule module);
        AutofacServiceProvider Build();
    }
}