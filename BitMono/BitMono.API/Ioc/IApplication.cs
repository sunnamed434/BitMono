using Autofac;

namespace BitMono.API.Ioc
{
    public interface IApplication
    {
        IContainer BuildContainer();
    }
}