using BitMono.Host.Ioc;
using BitMono.Shared.DependencyInjection;

namespace BitMono.Host;

/// <summary>
/// Main application class for building the BitMono DI container.
/// </summary>
public class BitMonoApplication : IApplication
{
    private readonly Container _container;
    private readonly List<IModule> _modules;

    public BitMonoApplication()
    {
        _container = new Container();
        _modules = [];
    }

    /// <inheritdoc/>
    public IApplication RegisterModule(IModule module)
    {
        _modules.Add(module);
        return this;
    }

    /// <inheritdoc/>
    public Task<IBitMonoServiceProvider> BuildAsync(CancellationToken cancellationToken)
    {
        foreach (var module in _modules)
        {
            cancellationToken.ThrowIfCancellationRequested();
            module.Load(_container);
        }

        // Register the container itself as the service provider
        _container.Register<IBitMonoServiceProvider>(_container).AsSingleton();

        return Task.FromResult<IBitMonoServiceProvider>(_container);
    }
}
