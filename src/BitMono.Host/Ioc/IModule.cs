using BitMono.Shared.DependencyInjection;

namespace BitMono.Host.Ioc;

/// <summary>
/// Interface for modules that configure the DI container.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Configures service registrations in the container.
    /// </summary>
    /// <param name="container">The container to configure</param>
    void Load(Container container);
}
