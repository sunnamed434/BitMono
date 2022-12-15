namespace BitMono.API.Ioc;

public interface IApplication
{
    IApplication Populate(IEnumerable<ServiceDescriptor> descriptors);
    IApplication RegisterModule(IModule module);
    AutofacServiceProvider Build();
}