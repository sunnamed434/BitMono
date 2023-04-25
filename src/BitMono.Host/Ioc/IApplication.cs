namespace BitMono.Host.Ioc;

public interface IApplication
{
    IApplication Populate(IEnumerable<ServiceDescriptor> descriptors);
    IApplication RegisterModule(IModule module);
    AutofacServiceProvider Build();
}