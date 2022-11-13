using Autofac;
using System;

namespace BitMono.Core.Ioc
{
    public static class LifetimeScopeExtensions
    {
        public static ILifetimeScope BeginLifetimeScopeEx(this ILifetimeScope source, Action<ContainerBuilder> configurationAction)
        {
            var lifetimeScope = source.BeginLifetimeScope(configurationAction);
            source.Disposer.AddInstanceForAsyncDisposal(lifetimeScope);
            return lifetimeScope;
        }
    }
}