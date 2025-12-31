using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace BitMono.Shared.DependencyInjection;

/// <summary>
/// Lightweight inversion of control container for dependency injection.
/// Ported from Microsoft MinIoC with enhancements.
/// </summary>
public class Container : Container.IScope
{
    #region Public interfaces
    /// <summary>
    /// Represents a scope in which per-scope objects are instantiated a single time
    /// </summary>
    public interface IScope : IDisposable, IBitMonoServiceProvider
    {
    }

    /// <summary>
    /// IRegisteredType is returned by Container.Register and allows further configuration for the registration
    /// </summary>
    public interface IRegisteredType
    {
        /// <summary>
        /// Make registered type a singleton
        /// </summary>
        void AsSingleton();

        /// <summary>
        /// Make registered type a per-scope type (single instance within a Scope)
        /// </summary>
        void PerScope();
    }
    #endregion

    // Map of registered types
    private readonly Dictionary<Type, Func<ILifetime, object>> _registeredTypes = new();

    // Lifetime management
    private readonly ContainerLifetime _lifetime;

    /// <summary>
    /// Creates a new instance of IoC Container
    /// </summary>
    public Container() => _lifetime = new ContainerLifetime(t => _registeredTypes[t]);

    /// <summary>
    /// Registers a factory function which will be called to resolve the specified interface
    /// </summary>
    /// <param name="interface">Interface to register</param>
    /// <param name="factory">Factory function</param>
    /// <returns></returns>
    public IRegisteredType Register(Type @interface, Func<object> factory)
        => RegisterType(@interface, _ => factory());

    /// <summary>
    /// Registers an implementation type for the specified interface
    /// </summary>
    /// <param name="interface">Interface to register</param>
    /// <param name="implementation">Implementing type</param>
    /// <returns></returns>
    public IRegisteredType Register(Type @interface, Type implementation)
    {
        // Handle self-registration (concrete type used as interface)
        if (@interface == implementation)
        {
            return RegisterType(@interface, FactoryFromType(implementation));
        }

        var interfaceRegistration = RegisterType(@interface, FactoryFromType(implementation));

        // Only register concrete type if not already registered
        if (!_registeredTypes.ContainsKey(implementation))
        {
            var concreteRegistration = RegisterType(implementation, FactoryFromType(implementation));
            return new CompositeRegisteredType(interfaceRegistration, concreteRegistration);
        }

        return interfaceRegistration;
    }

    /// <summary>
    /// Registers an implementation type for the specified interface
    /// </summary>
    /// <returns></returns>
    public IRegisteredType Register<TInterface, TImpl>()
        => Register(typeof(TInterface), typeof(TImpl));

    /// <summary>
    /// Registers an implementation type for itself and all of its implemented interfaces.
    /// </summary>
    /// <param name="implementation">Implementing type</param>
    /// <returns></returns>
    public IRegisteredType Register(Type implementation)
    {
        var factory = FactoryFromType(implementation);
        var concreteRegistration = RegisterType(implementation, factory);
        var interfaces = implementation.GetInterfaces();
        foreach (var interfaceType in interfaces)
        {
            RegisterType(interfaceType, factory);
        }
        return concreteRegistration;
    }

    /// <summary>
    /// Registers an implementation type for the specified type
    /// </summary>
    /// <returns></returns>
    public IRegisteredType Register<TImpl>()
    {
        return Register(typeof(TImpl));
    }

    /// <summary>
    /// Registers a factory function which will be called to resolve the specified interface
    /// </summary>
    /// <param name="factory">Factory function</param>
    /// <returns></returns>
    public IRegisteredType Register<TImpl>(Func<object> factory)
        => RegisterType(typeof(TImpl), _ => factory());

    /// <summary>
    /// Registers the given instance as the service for T.
    /// You can still call .AsSingleton() or .PerScope() on the returned IRegisteredType,
    /// but since it's a concrete instance, you will almost always want AsSingleton().
    /// </summary>
    public IRegisteredType Register<T>(T instance)
    {
        return Register(typeof(T), () => instance!);
    }

    /// <summary>
    /// Checks if a type is registered in the container
    /// </summary>
    public bool IsRegistered(Type type) => _registeredTypes.ContainsKey(type);

    /// <summary>
    /// Checks if a type is registered in the container
    /// </summary>
    public bool IsRegistered<T>() => IsRegistered(typeof(T));

    private IRegisteredType RegisterType(Type itemType, Func<ILifetime, object> factory)
        => new RegisteredType(itemType, f => _registeredTypes[itemType] = f, factory);

    /// <summary>
    /// Returns the object registered for the given type, if registered
    /// </summary>
    /// <param name="type">Type as registered with the container</param>
    /// <returns>Instance of the registered type, if registered; otherwise null</returns>
    public object? GetService(Type type)
    {
        if (!_registeredTypes.TryGetValue(type, out var registeredType))
        {
            return null;
        }
        return registeredType(_lifetime);
    }

    /// <summary>
    /// Creates a new scope
    /// </summary>
    /// <returns>Scope object</returns>
    public IScope CreateScope() => new ScopeLifetime(_lifetime);

    /// <summary>
    /// Disposes any IDisposable objects owned by this container.
    /// </summary>
    public void Dispose() => _lifetime.Dispose();

    #region Lifetime management
    // ILifetime management adds resolution strategies to an IScope
    interface ILifetime : IScope
    {
        object GetServiceAsSingleton(Type type, Func<ILifetime, object> factory);
        object GetServicePerScope(Type type, Func<ILifetime, object> factory);
    }

    // ObjectCache provides common caching logic for lifetimes
    abstract class ObjectCache
    {
        // Instance cache
        private readonly ConcurrentDictionary<Type, object> _instanceCache = [];

        // Track types currently being resolved to detect circular dependencies
        private readonly ThreadLocal<HashSet<Type>> _resolutionStack = new(() => []);

        // Get from cache or create and cache object
        protected object GetCached(Type type, Func<ILifetime, object> factory, ILifetime lifetime)
            => _instanceCache.GetOrAdd(type, _ => factory(lifetime));

        // Circular dependency detection methods
        public void EnterResolution(Type type)
        {
            if (!_resolutionStack.Value!.Add(type))
            {
                var circularChain = string.Join(" -> ", _resolutionStack.Value.Select(x => x.Name));
                throw new InvalidOperationException($"Circular dependency: {circularChain} -> {type.Name}");
            }
        }

        public void ExitResolution(Type type)
        {
            _resolutionStack.Value!.Remove(type);
        }

        public void Dispose()
        {
            foreach (var obj in _instanceCache.Values)
            {
                if (obj is IBitMonoServiceProvider or IScope)
                    continue;
                (obj as IDisposable)?.Dispose();
            }
            _resolutionStack.Dispose();
        }
    }

    // Container lifetime management
    class ContainerLifetime : ObjectCache, ILifetime
    {
        // Retrieves the factory function from the given type, provided by owning container
        public Func<Type, Func<ILifetime, object>> GetFactory { get; private set; }

        public ContainerLifetime(Func<Type, Func<ILifetime, object>> getFactory) => GetFactory = getFactory;

        public object GetService(Type type)
        {
            try
            {
                EnterResolution(type);
                return GetFactory(type)(this);
            }
            finally
            {
                ExitResolution(type);
            }
        }

        // Singletons get cached per container
        public object GetServiceAsSingleton(Type type, Func<ILifetime, object> factory)
            => GetCached(type, factory, this);

        // At container level, per-scope items are equivalent to singletons
        public object GetServicePerScope(Type type, Func<ILifetime, object> factory)
            => GetServiceAsSingleton(type, factory);
    }

    // Per-scope lifetime management
    class ScopeLifetime : ObjectCache, ILifetime
    {
        // Singletons come from parent container's lifetime
        private readonly ContainerLifetime _parentLifetime;

        public ScopeLifetime(ContainerLifetime parentContainer) => _parentLifetime = parentContainer;

        public object GetService(Type type)
        {
            try
            {
                EnterResolution(type);
                return _parentLifetime.GetFactory(type)(this);
            }
            finally
            {
                ExitResolution(type);
            }
        }

        // Singleton resolution is delegated to parent lifetime
        public object GetServiceAsSingleton(Type type, Func<ILifetime, object> factory)
            => _parentLifetime.GetServiceAsSingleton(type, factory);

        // Per-scope objects get cached
        public object GetServicePerScope(Type type, Func<ILifetime, object> factory)
            => GetCached(type, factory, this);
    }
    #endregion

    #region Container items
    // Compiles a lambda that calls the given type's first constructor resolving arguments
    private static Func<ILifetime, object> FactoryFromType(Type itemType)
    {
        // Get first constructor for the type
        var constructors = itemType.GetConstructors();
        if (constructors.Length == 0)
        {
            // If no public constructor found, search for an internal constructor
            constructors = itemType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
        }
        var constructor = constructors.First();

        // Compile constructor call as a lambda expression
        var arg = Expression.Parameter(typeof(ILifetime));
        return (Func<ILifetime, object>)Expression.Lambda(
            Expression.New(constructor, constructor.GetParameters().Select(
                param =>
                {
                    var resolve = new Func<ILifetime, object>(
                        lifetime => lifetime.GetService(param.ParameterType)!);
                    return Expression.Convert(
                        Expression.Call(Expression.Constant(resolve.Target), resolve.Method, arg),
                        param.ParameterType);
                })),
            arg).Compile();
    }

    // RegisteredType is supposed to be a short lived object tying an item to its container
    // and allowing users to mark it as a singleton or per-scope item
    class RegisteredType : IRegisteredType
    {
        private readonly Type _itemType;
        private readonly Action<Func<ILifetime, object>> _registerFactory;
        private readonly Func<ILifetime, object> _factory;

        public RegisteredType(Type itemType, Action<Func<ILifetime, object>> registerFactory, Func<ILifetime, object> factory)
        {
            _itemType = itemType;
            _registerFactory = registerFactory;
            _factory = factory;

            registerFactory(_factory);
        }

        public void AsSingleton()
            => _registerFactory(lifetime => lifetime.GetServiceAsSingleton(_itemType, _factory));

        public void PerScope()
            => _registerFactory(lifetime => lifetime.GetServicePerScope(_itemType, _factory));
    }

    class CompositeRegisteredType : IRegisteredType
    {
        private readonly IRegisteredType _first;
        private readonly IRegisteredType _second;

        public CompositeRegisteredType(IRegisteredType first, IRegisteredType second)
        {
            _first = first;
            _second = second;
        }

        public void AsSingleton()
        {
            _first.AsSingleton();
            _second.AsSingleton();
        }

        public void PerScope()
        {
            _first.PerScope();
            _second.PerScope();
        }
    }
    #endregion
}
