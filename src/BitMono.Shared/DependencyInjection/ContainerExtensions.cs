using System.Reflection;

namespace BitMono.Shared.DependencyInjection;

/// <summary>
/// Extension methods for Container
/// </summary>
public static class ContainerExtensions
{
    /// <summary>
    /// Returns an implementation of the specified interface
    /// </summary>
    /// <typeparam name="T">Interface type</typeparam>
    /// <param name="provider">Service provider instance</param>
    /// <returns>Object implementing the interface</returns>
    public static T Resolve<T>(this IBitMonoServiceProvider provider)
        => (T)provider.GetService(typeof(T))!;

    /// <summary>
    /// Returns an implementation of the specified interface, or throws if not found
    /// </summary>
    /// <typeparam name="T">Interface type</typeparam>
    /// <param name="provider">Service provider instance</param>
    /// <returns>Object implementing the interface</returns>
    /// <exception cref="InvalidOperationException">Thrown when service is not registered</exception>
    public static T GetRequiredService<T>(this IBitMonoServiceProvider provider)
    {
        var service = provider.GetService(typeof(T));
        if (service == null)
            throw new InvalidOperationException($"Service of type '{typeof(T).FullName}' is not registered.");
        return (T)service;
    }

    /// <summary>
    /// Returns an implementation of the specified interface, or default if not found
    /// </summary>
    /// <typeparam name="T">Interface type</typeparam>
    /// <param name="provider">Service provider instance</param>
    /// <returns>Object implementing the interface, or default</returns>
    public static T? GetService<T>(this IBitMonoServiceProvider provider)
    {
        var service = provider.GetService(typeof(T));
        return service == null ? default : (T)service;
    }

    /// <summary>
    /// Registers an implementation type for the specified interface
    /// </summary>
    /// <typeparam name="T">Interface to register</typeparam>
    /// <param name="container">This container instance</param>
    /// <param name="type">Implementing type</param>
    /// <returns>IRegisteredType object</returns>
    public static Container.IRegisteredType Register<T>(this Container container, Type type)
        => container.Register(typeof(T), type);

    /// <summary>
    /// Registers an implementation type for the specified interface
    /// </summary>
    /// <typeparam name="TInterface">Interface to register</typeparam>
    /// <typeparam name="TImplementation">Implementing type</typeparam>
    /// <param name="container">This container instance</param>
    /// <returns>IRegisteredType object</returns>
    public static Container.IRegisteredType Register<TInterface, TImplementation>(this Container container)
        where TImplementation : TInterface
        => container.Register(typeof(TInterface), typeof(TImplementation));

    /// <summary>
    /// Registers a factory function which will be called to resolve the specified interface
    /// </summary>
    /// <typeparam name="T">Interface to register</typeparam>
    /// <param name="container">This container instance</param>
    /// <param name="factory">Factory method</param>
    /// <returns>IRegisteredType object</returns>
    public static Container.IRegisteredType Register<T>(this Container container, Func<T> factory)
        => container.Register(typeof(T), () => factory()!);

    /// <summary>
    /// Registers all public types in assemblies implementing the specified interface.
    /// </summary>
    /// <param name="container">Container instance</param>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <param name="interfaceType">Interface type to look for</param>
    /// <param name="filter">Optional filter function</param>
    /// <returns>Container for chaining</returns>
    public static Container RegisterAssemblyTypes(
        this Container container,
        Assembly[] assemblies,
        Type interfaceType,
        Func<Type, bool>? filter = null)
    {
        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract || !type.IsPublic)
                    continue;

                if (filter != null && !filter(type))
                    continue;

                if (type.GetInterface(interfaceType.Name) != null)
                {
                    container.Register(interfaceType, type).AsSingleton();
                }
            }
        }
        return container;
    }

    /// <summary>
    /// Registers all closed types of an open generic interface.
    /// For example, registers all implementations of ICriticalAnalyzer&lt;T&gt;.
    /// </summary>
    /// <param name="container">Container instance</param>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <param name="openGenericType">Open generic type (e.g. typeof(ICriticalAnalyzer&lt;&gt;))</param>
    /// <param name="filter">Optional filter function</param>
    /// <returns>Container for chaining</returns>
    public static Container RegisterClosedTypesOf(
        this Container container,
        Assembly[] assemblies,
        Type openGenericType,
        Func<Type, bool>? filter = null)
    {
        if (!openGenericType.IsGenericTypeDefinition)
            throw new ArgumentException($"Type {openGenericType.Name} must be an open generic type definition.");

        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract || !type.IsPublic)
                    continue;

                if (filter != null && !filter(type))
                    continue;

                // Find all interfaces that are closed versions of the open generic type
                var implementedInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericType);

                foreach (var closedInterface in implementedInterfaces)
                {
                    container.Register(closedInterface, type).AsSingleton();
                }
            }
        }
        return container;
    }

    /// <summary>
    /// Registers all types implementing an interface and allows resolving them as a collection.
    /// </summary>
    /// <typeparam name="T">Interface type</typeparam>
    /// <param name="container">Container instance</param>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <param name="filter">Optional filter function</param>
    /// <returns>Container for chaining</returns>
    public static Container RegisterCollection<T>(
        this Container container,
        Assembly[] assemblies,
        Func<Type, bool>? filter = null) where T : class
    {
        var interfaceType = typeof(T);
        var implementations = new List<Type>();

        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract || !type.IsPublic)
                    continue;

                if (filter != null && !filter(type))
                    continue;

                if (interfaceType.IsAssignableFrom(type))
                {
                    implementations.Add(type);
                    // Register individual type as well
                    container.Register(type, type).AsSingleton();
                }
            }
        }

        // Register the collection factory
        container.Register<ICollection<T>>(() =>
        {
            var list = new List<T>();
            foreach (var implType in implementations)
            {
                var instance = container.GetService(implType);
                if (instance is T typedInstance)
                {
                    list.Add(typedInstance);
                }
            }
            return list;
        }).AsSingleton();

        return container;
    }
}
