namespace BitMono.Shared.DependencyInjection;

/// <summary>
/// Custom service provider interface to resolve services from the container.
/// </summary>
public interface IBitMonoServiceProvider
{
    /// <summary>
    /// Returns the object registered for the given type, if registered.
    /// </summary>
    /// <param name="serviceType">Type as registered with the container</param>
    /// <returns>Instance of the registered type, if registered; otherwise null</returns>
    object? GetService(Type serviceType);
}
