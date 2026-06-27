using System.Reflection;

namespace BitMono.Shared.Tests;

public interface ITestResolver { }

public class ConcreteTestResolver : ITestResolver { }

public class AnotherTestResolver : ITestResolver { }

public interface IGenericTestResolver<T> { }

public class GenericTestResolverBase<T> : IGenericTestResolver<T> { }

public class StringTestResolver : GenericTestResolverBase<string> { }

public class IntTestResolver : GenericTestResolverBase<int> { }

public interface ITestAnalyzer<T> { }

public abstract class TestAnalyzerBase<T> : ITestAnalyzer<T> { }

public class StringTestAnalyzer : TestAnalyzerBase<string> { }

public class IntTestAnalyzer : ITestAnalyzer<int> { }

/// <summary>
/// Simulates the AttributeResolver&lt;TModel&gt; scenario from
/// https://github.com/sunnamed434/BitMono/issues/268
/// </summary>
public class OpenGenericTestImplementation<T> : IGenericTestResolver<T> { }

public class ContainerExtensionsTests
{
    [Fact]
    public void RegisterAssemblyTypes_ShouldRegisterImplementations()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterAssemblyTypes(assemblies, typeof(ITestResolver));

        container.IsRegistered<ITestResolver>().ShouldBeTrue();
    }

    [Fact]
    public void RegisterAssemblyTypes_ShouldSkipAbstractClasses()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterAssemblyTypes(assemblies, typeof(ITestAnalyzer<string>));

        // Should not throw - abstract TestAnalyzerBase should be skipped
        var service = container.GetService(typeof(ITestAnalyzer<string>));
        service.ShouldNotBeNull();
    }

    [Fact]
    public void RegisterAssemblyTypes_ShouldSkipGenericTypeDefinitions()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        // This should not throw - generic type definitions should be skipped
        var act = () => container.RegisterAssemblyTypes(assemblies, typeof(IGenericTestResolver<string>));

        act.ShouldNotThrow();
    }

    [Fact]
    public void RegisterAssemblyTypes_WithFilter_ShouldApplyFilter()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterAssemblyTypes(assemblies, typeof(ITestResolver),
            type => type.Name.StartsWith("Concrete"));

        var service = container.GetService(typeof(ITestResolver));
        service.ShouldBeOfType<ConcreteTestResolver>();
    }

    [Fact]
    public void RegisterClosedTypesOf_ShouldRegisterClosedGenericImplementations()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterClosedTypesOf(assemblies, typeof(ITestAnalyzer<>));

        container.IsRegistered(typeof(ITestAnalyzer<string>)).ShouldBeTrue();
        container.IsRegistered(typeof(ITestAnalyzer<int>)).ShouldBeTrue();
    }

    [Fact]
    public void RegisterClosedTypesOf_ShouldSkipAbstractClasses()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        // TestAnalyzerBase<T> is abstract and should be skipped
        var act = () => container.RegisterClosedTypesOf(assemblies, typeof(ITestAnalyzer<>));

        act.ShouldNotThrow();
    }

    /// <summary>
    /// Regression test for https://github.com/sunnamed434/BitMono/issues/268
    /// OpenGenericTestImplementation&lt;T&gt; is a generic type definition and should be skipped.
    /// </summary>
    [Fact]
    public void RegisterClosedTypesOf_ShouldSkipGenericTypeDefinitions()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        var act = () => container.RegisterClosedTypesOf(assemblies, typeof(IGenericTestResolver<>));

        act.ShouldNotThrow();
    }

    [Fact]
    public void RegisterClosedTypesOf_ShouldResolveRegisteredTypes()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterClosedTypesOf(assemblies, typeof(ITestAnalyzer<>));

        var stringAnalyzer = container.GetService(typeof(ITestAnalyzer<string>));
        var intAnalyzer = container.GetService(typeof(ITestAnalyzer<int>));

        stringAnalyzer.ShouldNotBeNull();
        stringAnalyzer.ShouldBeOfType<StringTestAnalyzer>();
        intAnalyzer.ShouldNotBeNull();
        intAnalyzer.ShouldBeOfType<IntTestAnalyzer>();
    }

    [Fact]
    public void RegisterClosedTypesOf_ShouldThrowForNonGenericType()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        var act = () => container.RegisterClosedTypesOf(assemblies, typeof(ITestResolver));

        act.ShouldThrow<ArgumentException>().Message.ShouldContain("open generic type");
    }

    [Fact]
    public void RegisterClosedTypesOf_WithFilter_ShouldApplyFilter()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterClosedTypesOf(assemblies, typeof(ITestAnalyzer<>),
            type => type.Name.StartsWith("String"));

        container.IsRegistered(typeof(ITestAnalyzer<string>)).ShouldBeTrue();
        container.IsRegistered(typeof(ITestAnalyzer<int>)).ShouldBeFalse();
    }

    /// <summary>
    /// Ensures concrete types are also registered, not just interfaces.
    /// This is needed when other services depend on concrete analyzer types (e.g., Renamer depends on NameCriticalAnalyzer).
    /// </summary>
    [Fact]
    public void RegisterClosedTypesOf_ShouldAlsoRegisterConcreteTypes()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterClosedTypesOf(assemblies, typeof(ITestAnalyzer<>));

        // Should be able to resolve by interface
        container.IsRegistered(typeof(ITestAnalyzer<string>)).ShouldBeTrue();

        // Should ALSO be able to resolve by concrete type (this is the fix)
        container.IsRegistered(typeof(StringTestAnalyzer)).ShouldBeTrue();
        container.IsRegistered(typeof(IntTestAnalyzer)).ShouldBeTrue();
    }

    [Fact]
    public void RegisterCollection_ShouldRegisterAllImplementations()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterCollection<ITestResolver>(assemblies);

        var collection = container.GetService(typeof(ICollection<ITestResolver>)) as ICollection<ITestResolver>;

        collection.ShouldNotBeNull();
        collection!.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void RegisterCollection_ShouldContainAllImplementations()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterCollection<ITestResolver>(assemblies);

        var collection = container.Resolve<ICollection<ITestResolver>>();

        collection.ShouldContain(r => r is ConcreteTestResolver);
        collection.ShouldContain(r => r is AnotherTestResolver);
    }

    [Fact]
    public void RegisterCollection_ShouldSkipGenericTypeDefinitions()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        // Should not throw when encountering generic type definitions
        var act = () => container.RegisterCollection<ITestResolver>(assemblies);

        act.ShouldNotThrow();
    }

    [Fact]
    public void RegisterCollection_WithFilter_ShouldApplyFilter()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterCollection<ITestResolver>(assemblies,
            type => type.Name.StartsWith("Concrete"));

        var collection = container.Resolve<ICollection<ITestResolver>>();

        collection.Count.ShouldBe(1);
        collection.ShouldAllBe(e => e is ConcreteTestResolver);
    }

    [Fact]
    public void RegisterCollection_ShouldAlsoRegisterIndividualTypes()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterCollection<ITestResolver>(assemblies);

        container.IsRegistered<ConcreteTestResolver>().ShouldBeTrue();
        container.IsRegistered<AnotherTestResolver>().ShouldBeTrue();
    }
}
