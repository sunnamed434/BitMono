namespace BitMono.Host;

public class BitMonoApplication : IApplication
{
    private readonly ContainerBuilder m_ContainerBuilder;
    private readonly List<IModule> m_Modules;

    public BitMonoApplication()
    {
        m_ContainerBuilder = new ContainerBuilder();
        m_Modules = new List<IModule>();
    }

    public IApplication Populate(IEnumerable<ServiceDescriptor> descriptors)
    {
        m_ContainerBuilder.Populate(descriptors);
        return this;
    }
    public IApplication RegisterModule(IModule module)
    {
        m_Modules.Add(module);
        return this;
    }
    [SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
    public AutofacServiceProvider Build()
    {
        for (var i = 0; i < m_Modules.Count; i++)
        {
            m_ContainerBuilder.RegisterModule(m_Modules[i]);
        }
        var container = m_ContainerBuilder.Build();
        return new AutofacServiceProvider(container.Resolve<ILifetimeScope>());
    }
}