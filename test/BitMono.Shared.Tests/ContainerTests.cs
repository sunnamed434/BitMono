namespace BitMono.Shared.Tests;

public class ContainerTests
{
    public interface IService { }
    public interface IOtherService { }
    public interface IDisposableService : IDisposable { }

    public class ServiceImpl : IService { }

    public class OtherServiceImpl : IOtherService { }

    public class ServiceWithDependency : IService
    {
        public IOtherService OtherService { get; }

        public ServiceWithDependency(IOtherService otherService)
        {
            OtherService = otherService;
        }
    }

    public class ServiceWithMultipleDependencies : IService
    {
        public IOtherService OtherService { get; }
        public IDisposableService DisposableService { get; }

        public ServiceWithMultipleDependencies(IOtherService otherService, IDisposableService disposableService)
        {
            OtherService = otherService;
            DisposableService = disposableService;
        }
    }

    public class DisposableServiceImpl : IDisposableService
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public class CircularA
    {
        public CircularA(CircularB b) { }
    }

    public class CircularB
    {
        public CircularB(CircularA a) { }
    }

    public interface IGenericService<T> { }

    public class GenericServiceImpl<T> : IGenericService<T> { }

    public class StringGenericService : IGenericService<string> { }

    public class IntGenericService : IGenericService<int> { }

    [Fact]
    public void SimpleReflectionConstruction_ShouldResolveType()
    {
        using var container = new Container();
        container.Register<IService, ServiceImpl>();

        var service = container.GetService(typeof(IService));

        service.ShouldNotBeNull();
        service.ShouldBeOfType<ServiceImpl>();
    }

    [Fact]
    public void RecursiveReflectionConstruction_ShouldResolveDependencies()
    {
        using var container = new Container();
        container.Register<IService, ServiceWithDependency>();
        container.Register<IOtherService, OtherServiceImpl>();

        var service = container.GetService(typeof(IService)) as ServiceWithDependency;

        service.ShouldNotBeNull();
        service!.OtherService.ShouldNotBeNull();
        service.OtherService.ShouldBeOfType<OtherServiceImpl>();
    }

    [Fact]
    public void SimpleFactoryConstruction_ShouldUseFactory()
    {
        using var container = new Container();
        var factoryCalled = false;
        container.Register<IService>(() =>
        {
            factoryCalled = true;
            return new ServiceImpl();
        });

        var service = container.GetService(typeof(IService));

        service.ShouldNotBeNull();
        factoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void MixedConstruction_ShouldCombineFactoryAndReflection()
    {
        using var container = new Container();
        container.Register<IService, ServiceWithDependency>();
        container.Register<IOtherService>(() => new OtherServiceImpl());

        var service = container.GetService(typeof(IService)) as ServiceWithDependency;

        service.ShouldNotBeNull();
        service!.OtherService.ShouldNotBeNull();
    }

    [Fact]
    public void InstanceRegistration_ShouldReturnSameInstance()
    {
        using var container = new Container();
        var instance = new ServiceImpl();
        container.Register<IService>(instance).AsSingleton();

        var resolved = container.GetService(typeof(IService));

        resolved.ShouldBeSameAs(instance);
    }

    [Fact]
    public void GenericRegistration_ShouldResolveGenericType()
    {
        using var container = new Container();
        container.Register<IService, ServiceImpl>();

        var service = container.Resolve<IService>();

        service.ShouldNotBeNull();
        service.ShouldBeOfType<ServiceImpl>();
    }

    [Fact]
    public void TransientResolution_ShouldCreateNewInstances()
    {
        using var container = new Container();
        container.Register<IService, ServiceImpl>();

        var service1 = container.GetService(typeof(IService));
        var service2 = container.GetService(typeof(IService));

        service1.ShouldNotBeSameAs(service2);
    }

    [Fact]
    public void SingletonResolution_ShouldReturnSameInstance()
    {
        using var container = new Container();
        container.Register<IService, ServiceImpl>().AsSingleton();

        var service1 = container.GetService(typeof(IService));
        var service2 = container.GetService(typeof(IService));

        service1.ShouldBeSameAs(service2);
    }

    [Fact]
    public void PerScopeResolution_ShouldReturnSameInstanceWithinScope()
    {
        using var container = new Container();
        container.Register<IService, ServiceImpl>().PerScope();

        using var scope = container.CreateScope();
        var service1 = scope.GetService(typeof(IService));
        var service2 = scope.GetService(typeof(IService));

        service1.ShouldBeSameAs(service2);
    }

    [Fact]
    public void PerScopeResolution_ShouldReturnDifferentInstancesAcrossScopes()
    {
        using var container = new Container();
        container.Register<IService, ServiceImpl>().PerScope();

        using var scope1 = container.CreateScope();
        using var scope2 = container.CreateScope();
        var service1 = scope1.GetService(typeof(IService));
        var service2 = scope2.GetService(typeof(IService));

        service1.ShouldNotBeSameAs(service2);
    }

    [Fact]
    public void SingletonInScope_ShouldReturnSameInstanceAsContainer()
    {
        using var container = new Container();
        container.Register<IService, ServiceImpl>().AsSingleton();

        var containerService = container.GetService(typeof(IService));
        using var scope = container.CreateScope();
        var scopeService = scope.GetService(typeof(IService));

        containerService.ShouldBeSameAs(scopeService);
    }

    [Fact]
    public void SingletonsAreDifferentAcrossContainers()
    {
        using var container1 = new Container();
        using var container2 = new Container();
        container1.Register<IService, ServiceImpl>().AsSingleton();
        container2.Register<IService, ServiceImpl>().AsSingleton();

        var service1 = container1.GetService(typeof(IService));
        var service2 = container2.GetService(typeof(IService));

        service1.ShouldNotBeSameAs(service2);
    }

    [Fact]
    public void ContainerDispose_ShouldDisposeSingletons()
    {
        DisposableServiceImpl? disposable = null;
        var container = new Container();
        container.Register<IDisposableService>(() =>
        {
            disposable = new DisposableServiceImpl();
            return disposable;
        }).AsSingleton();

        container.GetService(typeof(IDisposableService));
        container.Dispose();

        disposable.ShouldNotBeNull();
        disposable!.IsDisposed.ShouldBeTrue();
    }

    [Fact]
    public void ScopeDispose_ShouldDisposePerScopeInstances()
    {
        using var container = new Container();
        DisposableServiceImpl? disposable = null;
        container.Register<IDisposableService>(() =>
        {
            disposable = new DisposableServiceImpl();
            return disposable;
        }).PerScope();

        var scope = container.CreateScope();
        scope.GetService(typeof(IDisposableService));
        scope.Dispose();

        disposable.ShouldNotBeNull();
        disposable!.IsDisposed.ShouldBeTrue();
    }

    [Fact]
    public void GetServiceUnregisteredType_ShouldReturnNull()
    {
        using var container = new Container();

        var service = container.GetService(typeof(IService));

        service.ShouldBeNull();
    }

    [Fact]
    public void GetServiceMissingDependency_ShouldThrow()
    {
        using var container = new Container();
        container.Register<IService, ServiceWithDependency>();

        var act = () => container.GetService(typeof(IService));

        act.ShouldThrow<KeyNotFoundException>();
    }

    [Fact]
    public void CircularDependency_ShouldThrow()
    {
        using var container = new Container();
        container.Register<CircularA>();
        container.Register<CircularB>();

        var act = () => container.GetService(typeof(CircularA));

        act.ShouldThrow<InvalidOperationException>().Message.ShouldContain("Circular dependency");
    }

    [Fact]
    public void IsRegistered_ShouldReturnTrueForRegisteredType()
    {
        using var container = new Container();
        container.Register<IService, ServiceImpl>();

        container.IsRegistered<IService>().ShouldBeTrue();
        container.IsRegistered(typeof(IService)).ShouldBeTrue();
    }

    [Fact]
    public void IsRegistered_ShouldReturnFalseForUnregisteredType()
    {
        using var container = new Container();

        container.IsRegistered<IService>().ShouldBeFalse();
        container.IsRegistered(typeof(IService)).ShouldBeFalse();
    }

    [Fact]
    public void SelfRegistration_ShouldRegisterConcreteType()
    {
        using var container = new Container();
        container.Register<ServiceImpl>();

        var service = container.GetService(typeof(ServiceImpl));

        service.ShouldNotBeNull();
        service.ShouldBeOfType<ServiceImpl>();
    }

    [Fact]
    public void SelfRegistration_ShouldAlsoRegisterInterfaces()
    {
        using var container = new Container();
        container.Register<ServiceImpl>();

        var service = container.GetService(typeof(IService));

        service.ShouldNotBeNull();
        service.ShouldBeOfType<ServiceImpl>();
    }

    [Fact]
    public void MixedReversedRegistration_ShouldResolveRegardlessOfOrder()
    {
        using var container = new Container();
        container.Register<IService, ServiceWithDependency>();
        container.Register<IOtherService, OtherServiceImpl>();

        var service = container.GetService(typeof(IService)) as ServiceWithDependency;

        service.ShouldNotBeNull();
        service!.OtherService.ShouldNotBeNull();
    }

    [Fact]
    public void Resolve_ShouldReturnTypedService()
    {
        using var container = new Container();
        container.Register<IService, ServiceImpl>();

        var service = container.Resolve<IService>();

        service.ShouldNotBeNull();
        service.ShouldBeOfType<ServiceImpl>();
    }

    [Fact]
    public void GetRequiredService_ShouldReturnService()
    {
        using var container = new Container();
        container.Register<IService, ServiceImpl>();

        var service = container.GetRequiredService<IService>();

        service.ShouldNotBeNull();
    }

    [Fact]
    public void GetRequiredService_ShouldThrowWhenNotRegistered()
    {
        using var container = new Container();

        var act = () => container.GetRequiredService<IService>();

        act.ShouldThrow<InvalidOperationException>().Message.ShouldContain("not registered");
    }

    [Fact]
    public void GetService_ShouldReturnDefaultWhenNotRegistered()
    {
        using var container = new Container();

        var service = container.GetService<IService>();

        service.ShouldBeNull();
    }
}
