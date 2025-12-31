using BitMono.Shared.DependencyInjection;

namespace BitMono.Host.Ioc;

/// <summary>
/// Interface for the application builder.
/// </summary>
public interface IApplication
{
    /// <summary>
    /// Registers a module with the application.
    /// </summary>
    /// <param name="module">Module to register</param>
    /// <returns>This application for chaining</returns>
    IApplication RegisterModule(IModule module);

    /// <summary>
    /// Builds the container and returns the service provider.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Service provider</returns>
    Task<IBitMonoServiceProvider> BuildAsync(CancellationToken cancellationToken);
}
