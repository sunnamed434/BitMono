namespace BitMono.Host;

public class BitMonoApplication : IApplication
{
    private readonly ContainerBuilder _containerBuilder;
    private readonly List<IModule> _modules;

    public BitMonoApplication()
    {
        _containerBuilder = new ContainerBuilder();
        _modules = new List<IModule>();
    }

    public IApplication Populate(IEnumerable<ServiceDescriptor> descriptors)
    {
        _containerBuilder.Populate(descriptors);
        return this;
    }
    public IApplication RegisterModule(IModule module)
    {
        _modules.Add(module);
        return this;
    }
    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    public AutofacServiceProvider Build()
    {
        for (var i = 0; i < _modules.Count; i++)
        {
            var module = _modules[i];
            _containerBuilder.RegisterModule(module);
        }
        var container = _containerBuilder.Build();
        return new AutofacServiceProvider(container.Resolve<ILifetimeScope>());
    }
}