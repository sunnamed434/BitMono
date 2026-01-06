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

        container.IsRegistered<ITestResolver>().Should().BeTrue();
    }

    [Fact]
    public void RegisterAssemblyTypes_ShouldSkipAbstractClasses()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterAssemblyTypes(assemblies, typeof(ITestAnalyzer<string>));

        // Should not throw - abstract TestAnalyzerBase should be skipped
        var service = container.GetService(typeof(ITestAnalyzer<string>));
        service.Should().NotBeNull();
    }

    [Fact]
    public void RegisterAssemblyTypes_ShouldSkipGenericTypeDefinitions()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        // This should not throw - generic type definitions should be skipped
        var act = () => container.RegisterAssemblyTypes(assemblies, typeof(IGenericTestResolver<string>));

        act.Should().NotThrow();
    }

    [Fact]
    public void RegisterAssemblyTypes_WithFilter_ShouldApplyFilter()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterAssemblyTypes(assemblies, typeof(ITestResolver),
            type => type.Name.StartsWith("Concrete"));

        var service = container.GetService(typeof(ITestResolver));
        service.Should().BeOfType<ConcreteTestResolver>();
    }

    [Fact]
    public void RegisterClosedTypesOf_ShouldRegisterClosedGenericImplementations()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterClosedTypesOf(assemblies, typeof(ITestAnalyzer<>));

        container.IsRegistered(typeof(ITestAnalyzer<string>)).Should().BeTrue();
        container.IsRegistered(typeof(ITestAnalyzer<int>)).Should().BeTrue();
    }

    [Fact]
    public void RegisterClosedTypesOf_ShouldSkipAbstractClasses()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        // TestAnalyzerBase<T> is abstract and should be skipped
        var act = () => container.RegisterClosedTypesOf(assemblies, typeof(ITestAnalyzer<>));

        act.Should().NotThrow();
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

        act.Should().NotThrow();
    }

    [Fact]
    public void RegisterClosedTypesOf_ShouldResolveRegisteredTypes()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterClosedTypesOf(assemblies, typeof(ITestAnalyzer<>));

        var stringAnalyzer = container.GetService(typeof(ITestAnalyzer<string>));
        var intAnalyzer = container.GetService(typeof(ITestAnalyzer<int>));

        stringAnalyzer.Should().NotBeNull();
        stringAnalyzer.Should().BeOfType<StringTestAnalyzer>();
        intAnalyzer.Should().NotBeNull();
        intAnalyzer.Should().BeOfType<IntTestAnalyzer>();
    }

    [Fact]
    public void RegisterClosedTypesOf_ShouldThrowForNonGenericType()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        var act = () => container.RegisterClosedTypesOf(assemblies, typeof(ITestResolver));

        act.Should().Throw<ArgumentException>()
            .WithMessage("*open generic type*");
    }

    [Fact]
    public void RegisterClosedTypesOf_WithFilter_ShouldApplyFilter()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterClosedTypesOf(assemblies, typeof(ITestAnalyzer<>),
            type => type.Name.StartsWith("String"));

        container.IsRegistered(typeof(ITestAnalyzer<string>)).Should().BeTrue();
        container.IsRegistered(typeof(ITestAnalyzer<int>)).Should().BeFalse();
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
        container.IsRegistered(typeof(ITestAnalyzer<string>)).Should().BeTrue();

        // Should ALSO be able to resolve by concrete type (this is the fix)
        container.IsRegistered(typeof(StringTestAnalyzer)).Should().BeTrue();
        container.IsRegistered(typeof(IntTestAnalyzer)).Should().BeTrue();
    }

    [Fact]
    public void RegisterCollection_ShouldRegisterAllImplementations()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterCollection<ITestResolver>(assemblies);

        var collection = container.GetService(typeof(ICollection<ITestResolver>)) as ICollection<ITestResolver>;

        collection.Should().NotBeNull();
        collection!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RegisterCollection_ShouldContainAllImplementations()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterCollection<ITestResolver>(assemblies);

        var collection = container.Resolve<ICollection<ITestResolver>>();

        collection.Should().Contain(r => r is ConcreteTestResolver);
        collection.Should().Contain(r => r is AnotherTestResolver);
    }

    [Fact]
    public void RegisterCollection_ShouldSkipGenericTypeDefinitions()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        // Should not throw when encountering generic type definitions
        var act = () => container.RegisterCollection<ITestResolver>(assemblies);

        act.Should().NotThrow();
    }

    [Fact]
    public void RegisterCollection_WithFilter_ShouldApplyFilter()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterCollection<ITestResolver>(assemblies,
            type => type.Name.StartsWith("Concrete"));

        var collection = container.Resolve<ICollection<ITestResolver>>();

        collection.Count.Should().Be(1);
        collection.Should().AllBeOfType<ConcreteTestResolver>();
    }

    [Fact]
    public void RegisterCollection_ShouldAlsoRegisterIndividualTypes()
    {
        using var container = new Container();
        var assemblies = new[] { typeof(ContainerExtensionsTests).Assembly };

        container.RegisterCollection<ITestResolver>(assemblies);

        container.IsRegistered<ConcreteTestResolver>().Should().BeTrue();
        container.IsRegistered<AnotherTestResolver>().Should().BeTrue();
    }
}
