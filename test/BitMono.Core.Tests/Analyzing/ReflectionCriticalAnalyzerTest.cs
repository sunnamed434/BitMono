namespace BitMono.Core.Tests.Analyzing;

public class ReflectionCriticalAnalyzerTest
{
    private static ReflectionCriticalAnalyzer CreateAnalyzer(bool reflectionEnabled = true)
    {
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = reflectionEnabled
        };
        return Setup.ReflectionCriticalAnalyzer(obfuscation);
    }

    private static (ModuleDefinition module, TypeDefinition type) GetTestData()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        return (module, type);
    }

    [Fact]
    public void ShouldDetectSelfReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectMultipleReflectionCalls()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.Uses3Reflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectFieldReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesFieldReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectPrivateFieldReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesPrivateFieldReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectPropertyReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesPropertyReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectReadOnlyPropertyReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReadOnlyPropertyReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectEventReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesEventReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectGetMemberForMethod()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMemberForMethod));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectGetMemberForField()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMemberForField));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectGetMemberForProperty()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMemberForProperty));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectGetMemberForEvent()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMemberForEvent));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectVariableForMethodReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesVariableForMethodReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectVariableForFieldReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesVariableForFieldReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectVariableForPropertyReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesVariableForPropertyReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectVariableForEventReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesVariableForEventReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectTypeGetTypeFromHandle()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesTypeGetTypeFromHandle));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectLdtokenForType()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesTypeGetTypeFromHandle));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectMultipleReflectionTypes()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesAllReflectionTypes));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectReflectionWithBindingFlags()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionWithBindingFlags));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldNotDetectReflectionWhenDisabled()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var criticalAnalyzer = CreateAnalyzer(false);

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldNotDetectReflectionInNonReflectionMethod()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.VoidMethod));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldDetectComplexReflectionPatterns()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesComplexReflectionPatterns));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldCacheReflectedMethods()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var criticalAnalyzer = CreateAnalyzer();

        var result1 = criticalAnalyzer.NotCriticalToMakeChanges(method);
        result1.Should().BeFalse();

        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
        criticalAnalyzer.CachedMethods.Should().Contain(method);
    }

    [Fact]
    public void ShouldCacheReflectedFields()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesFieldReflection));
        var field = type.Fields.First(f => f.Name == nameof(ReflectionMethods.TestField));
        var criticalAnalyzer = CreateAnalyzer();

        var result1 = criticalAnalyzer.NotCriticalToMakeChanges(method);
        result1.Should().BeFalse();

        criticalAnalyzer.CachedFields.Should().NotBeEmpty();
        criticalAnalyzer.CachedFields.Should().Contain(field);
    }

    [Fact]
    public void ShouldCacheReflectedProperties()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesPropertyReflection));
        var property = type.Properties.First(p => p.Name == nameof(ReflectionMethods.TestProperty));
        var criticalAnalyzer = CreateAnalyzer();

        var result1 = criticalAnalyzer.NotCriticalToMakeChanges(method);
        result1.Should().BeFalse();

        criticalAnalyzer.CachedProperties.Should().NotBeEmpty();
        criticalAnalyzer.CachedProperties.Should().Contain(property);
    }

    [Fact]
    public void ShouldCacheReflectedEvents()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesEventReflection));
        var eventDef = type.Events.First(e => e.Name == nameof(ReflectionMethods.TestEvent));
        var criticalAnalyzer = CreateAnalyzer();

        var result1 = criticalAnalyzer.NotCriticalToMakeChanges(method);
        result1.Should().BeFalse();

        criticalAnalyzer.CachedEvents.Should().NotBeEmpty();
        criticalAnalyzer.CachedEvents.Should().Contain(eventDef);
    }

    [Fact]
    public void ShouldCacheReflectedTypes()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesTypeGetTypeFromHandle));
        var criticalAnalyzer = CreateAnalyzer();

        var result1 = criticalAnalyzer.NotCriticalToMakeChanges(method);
        result1.Should().BeFalse();

        criticalAnalyzer.CachedTypes.Should().NotBeEmpty();
        criticalAnalyzer.CachedTypes.Should().Contain(type);
    }

    [Fact]
    public void ShouldPreventDuplicateCacheEntries()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var criticalAnalyzer = CreateAnalyzer();

        var result1 = criticalAnalyzer.NotCriticalToMakeChanges(method);
        var result2 = criticalAnalyzer.NotCriticalToMakeChanges(method);
        var result3 = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result1.Should().BeFalse();
        result2.Should().BeFalse();
        result3.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().HaveCount(1);
        criticalAnalyzer.CachedMethods.Should().Contain(method);
    }

    [Fact]
    public void ShouldDetectCrossMethodReflection()
    {
        var (_, type) = GetTestData();
        var reflectionMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesFieldReflection));
        var targetMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var criticalAnalyzer = CreateAnalyzer();

        var reflectionResult = criticalAnalyzer.NotCriticalToMakeChanges(reflectionMethod);
        var result = criticalAnalyzer.NotCriticalToMakeChanges(targetMethod);

        reflectionResult.Should().BeFalse();
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldHandleFieldNotCriticalToMakeChanges()
    {
        var (_, type) = GetTestData();
        var field = type.Fields.First(f => f.Name == nameof(ReflectionMethods.TestField));
        var reflectionMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesFieldReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var reflectionResult = criticalAnalyzer.NotCriticalToMakeChanges(reflectionMethod);
        var result = criticalAnalyzer.NotCriticalToMakeChanges(field);

        reflectionResult.Should().BeFalse();
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldHandlePropertyNotCriticalToMakeChanges()
    {
        var (_, type) = GetTestData();
        var property = type.Properties.First(p => p.Name == nameof(ReflectionMethods.TestProperty));
        var reflectionMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesPropertyReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var reflectionResult = criticalAnalyzer.NotCriticalToMakeChanges(reflectionMethod);
        var result = criticalAnalyzer.NotCriticalToMakeChanges(property);

        reflectionResult.Should().BeFalse();
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldHandleEventNotCriticalToMakeChanges()
    {
        var (_, type) = GetTestData();
        var eventDef = type.Events.First(e => e.Name == nameof(ReflectionMethods.TestEvent));
        var reflectionMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesEventReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var reflectionResult = criticalAnalyzer.NotCriticalToMakeChanges(reflectionMethod);
        var result = criticalAnalyzer.NotCriticalToMakeChanges(eventDef);

        reflectionResult.Should().BeFalse();
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldHandleTypeNotCriticalToMakeChanges()
    {
        var (_, type) = GetTestData();
        var reflectionMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesTypeGetTypeFromHandle));
        var criticalAnalyzer = CreateAnalyzer();

        var reflectionResult = criticalAnalyzer.NotCriticalToMakeChanges(reflectionMethod);
        var result = criticalAnalyzer.NotCriticalToMakeChanges(type);

        reflectionResult.Should().BeFalse();
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldReturnTrueForNonReflectedMembers()
    {
        var (_, type) = GetTestData();
        var field = type.Fields.First(f => f.Name == nameof(ReflectionMethods.TestField));
        var property = type.Properties.First(p => p.Name == nameof(ReflectionMethods.TestProperty));
        var eventDef = type.Events.First(e => e.Name == nameof(ReflectionMethods.TestEvent));
        var criticalAnalyzer = CreateAnalyzer();

        criticalAnalyzer.NotCriticalToMakeChanges(field).Should().BeTrue();
        criticalAnalyzer.NotCriticalToMakeChanges(property).Should().BeTrue();
        criticalAnalyzer.NotCriticalToMakeChanges(eventDef).Should().BeTrue();
        criticalAnalyzer.NotCriticalToMakeChanges(type).Should().BeTrue();
    }

    [Fact]
    public void ShouldReturnTrueWhenReflectionDisabled()
    {
        var (_, type) = GetTestData();
        var field = type.Fields.First(f => f.Name == nameof(ReflectionMethods.TestField));
        var property = type.Properties.First(p => p.Name == nameof(ReflectionMethods.TestProperty));
        var eventDef = type.Events.First(e => e.Name == nameof(ReflectionMethods.TestEvent));
        var criticalAnalyzer = CreateAnalyzer(false);

        criticalAnalyzer.NotCriticalToMakeChanges(field).Should().BeTrue();
        criticalAnalyzer.NotCriticalToMakeChanges(property).Should().BeTrue();
        criticalAnalyzer.NotCriticalToMakeChanges(eventDef).Should().BeTrue();
        criticalAnalyzer.NotCriticalToMakeChanges(type).Should().BeTrue();
    }

    [Fact]
    public void ShouldHandleMultipleReflectionTypesInSingleMethod()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesAllReflectionTypes));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();

        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
        criticalAnalyzer.CachedFields.Should().NotBeEmpty();
        criticalAnalyzer.CachedProperties.Should().NotBeEmpty();
        criticalAnalyzer.CachedEvents.Should().NotBeEmpty();
        criticalAnalyzer.CachedTypes.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldHandleGetMemberReflectionForAllMemberTypes()
    {
        var (_, type) = GetTestData();
        var methodMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMemberForMethod));
        var fieldMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMemberForField));
        var propertyMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMemberForProperty));
        var eventMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMemberForEvent));
        var criticalAnalyzer = CreateAnalyzer();

        var methodResult = criticalAnalyzer.NotCriticalToMakeChanges(methodMethod);
        var fieldResult = criticalAnalyzer.NotCriticalToMakeChanges(fieldMethod);
        var propertyResult = criticalAnalyzer.NotCriticalToMakeChanges(propertyMethod);
        var eventResult = criticalAnalyzer.NotCriticalToMakeChanges(eventMethod);

        methodResult.Should().BeFalse();
        fieldResult.Should().BeFalse();
        propertyResult.Should().BeFalse();
        eventResult.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
        criticalAnalyzer.CachedFields.Should().NotBeEmpty();
        criticalAnalyzer.CachedProperties.Should().NotBeEmpty();
        criticalAnalyzer.CachedEvents.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldHandleVariableBasedReflection()
    {
        var (_, type) = GetTestData();
        var methodMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesVariableForMethodReflection));
        var fieldMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesVariableForFieldReflection));
        var propertyMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesVariableForPropertyReflection));
        var eventMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesVariableForEventReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var methodResult = criticalAnalyzer.NotCriticalToMakeChanges(methodMethod);
        var fieldResult = criticalAnalyzer.NotCriticalToMakeChanges(fieldMethod);
        var propertyResult = criticalAnalyzer.NotCriticalToMakeChanges(propertyMethod);
        var eventResult = criticalAnalyzer.NotCriticalToMakeChanges(eventMethod);

        methodResult.Should().BeFalse();
        fieldResult.Should().BeFalse();
        propertyResult.Should().BeFalse();
        eventResult.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
        criticalAnalyzer.CachedFields.Should().NotBeEmpty();
        criticalAnalyzer.CachedProperties.Should().NotBeEmpty();
        criticalAnalyzer.CachedEvents.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldNotDetectDeepReflectionInCallingMethod()
    {
        var (_, type) = GetTestData();
        var level1Method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.DeepReflectionLevel1));
        var level2Method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.DeepReflectionLevel2));
        var level3Method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.DeepReflectionLevel3));
        var reflectionMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var criticalAnalyzer = CreateAnalyzer();

        var level1Result = criticalAnalyzer.NotCriticalToMakeChanges(level1Method);
        var level2Result = criticalAnalyzer.NotCriticalToMakeChanges(level2Method);
        var level3Result = criticalAnalyzer.NotCriticalToMakeChanges(level3Method);
        var reflectionResult = criticalAnalyzer.NotCriticalToMakeChanges(reflectionMethod);

        level1Result.Should().BeTrue();
        level2Result.Should().BeTrue();
        level3Result.Should().BeTrue();
        reflectionResult.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectDirectReflectionInMethod()
    {
        var (_, type) = GetTestData();
        var directReflectionMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.DeepReflectionDirect));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(directReflectionMethod);

        result.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().Contain(directReflectionMethod);
    }

    [Fact]
    public void ShouldNotDetectNonReflectionMethods()
    {
        var (_, type) = GetTestData();
        var nonReflectionMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.NonReflectionMethod));
        var callsNonReflectionMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.CallsNonReflectionMethod));
        var criticalAnalyzer = CreateAnalyzer();

        var nonReflectionResult = criticalAnalyzer.NotCriticalToMakeChanges(nonReflectionMethod);
        var callsNonReflectionResult = criticalAnalyzer.NotCriticalToMakeChanges(callsNonReflectionMethod);

        nonReflectionResult.Should().BeTrue();
        callsNonReflectionResult.Should().BeTrue();
    }

    [Fact]
    public void ShouldVerifyDeepReflectionCallChain()
    {
        var (_, type) = GetTestData();
        var level1Method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.DeepReflectionLevel1));
        var level2Method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.DeepReflectionLevel2));
        var level3Method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.DeepReflectionLevel3));
        var reflectionMethod = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var criticalAnalyzer = CreateAnalyzer();

        criticalAnalyzer.NotCriticalToMakeChanges(level1Method);
        criticalAnalyzer.NotCriticalToMakeChanges(level2Method);
        criticalAnalyzer.NotCriticalToMakeChanges(level3Method);
        criticalAnalyzer.NotCriticalToMakeChanges(reflectionMethod);

        criticalAnalyzer.CachedMethods.Should().HaveCount(1);
        criticalAnalyzer.CachedMethods.Should().Contain(reflectionMethod);
        criticalAnalyzer.CachedMethods.Should().NotContain(level1Method);
        criticalAnalyzer.CachedMethods.Should().NotContain(level2Method);
        criticalAnalyzer.CachedMethods.Should().NotContain(level3Method);
    }


    [Fact]
    public void ShouldDetectBaseTypeReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesBaseTypeReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
        criticalAnalyzer.CachedFields.Should().NotBeEmpty();
        criticalAnalyzer.CachedProperties.Should().NotBeEmpty();
        criticalAnalyzer.CachedEvents.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectInheritedMemberReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesInheritedMemberReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectGetMethods()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMethods));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectGetFields()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetFields));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedFields.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectGetProperties()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetProperties));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedProperties.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectGetEvents()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetEvents));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedEvents.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectGetMembers()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMembers));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
        criticalAnalyzer.CachedFields.Should().NotBeEmpty();
        criticalAnalyzer.CachedProperties.Should().NotBeEmpty();
        criticalAnalyzer.CachedEvents.Should().NotBeEmpty();
        criticalAnalyzer.CachedTypes.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectGenericTypeReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGenericTypeReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
        criticalAnalyzer.CachedProperties.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectConstructedGenericTypeReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesConstructedGenericTypeReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
        criticalAnalyzer.CachedProperties.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectAssemblyGetType()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesAssemblyGetType));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectAssemblyGetTypeWithReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesAssemblyGetTypeWithReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
        criticalAnalyzer.CachedFields.Should().NotBeEmpty();
        criticalAnalyzer.CachedProperties.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectComplexTypeResolution()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesComplexTypeResolution));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
        criticalAnalyzer.CachedFields.Should().NotBeEmpty();
        criticalAnalyzer.CachedProperties.Should().NotBeEmpty();
        criticalAnalyzer.CachedEvents.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectNestedTypeReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesNestedTypeReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedTypes.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectInterfaceReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesInterfaceReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldDetectMemberOverrideReflection()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesMemberOverrideReflection));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
        criticalAnalyzer.CachedProperties.Should().NotBeEmpty();
        criticalAnalyzer.CachedEvents.Should().NotBeEmpty();
        
        var baseType = type.DeclaringModule.TopLevelTypes
            .First(t => t.Name == "ReflectionMethods")
            .NestedTypes.First(t => t.Name == "BaseClass");
        var derivedType = type.DeclaringModule.TopLevelTypes
            .First(t => t.Name == "ReflectionMethods")
            .NestedTypes.First(t => t.Name == "DerivedClass");
        var baseMethods = baseType.Methods.Where(m => m.Name == "BaseMethod" || m.Name == "VirtualMethod");
        foreach (var baseMethod in baseMethods)
        {
            criticalAnalyzer.CachedMethods.Should().Contain(baseMethod);
        }
        
        var baseProperties = baseType.Properties.Where(p => p.Name == "BaseProperty" || p.Name == "VirtualProperty");
        foreach (var baseProperty in baseProperties)
        {
            criticalAnalyzer.CachedProperties.Should().Contain(baseProperty);
        }
        
        var baseEvents = baseType.Events.Where(e => e.Name == "BaseEvent" || e.Name == "VirtualEvent");
        foreach (var baseEvent in baseEvents)
        {
            criticalAnalyzer.CachedEvents.Should().Contain(baseEvent);
        }
    }

    [Fact]
    public void ShouldHandleLegacyFallbackWhenArgumentTracingFails()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var criticalAnalyzer = CreateAnalyzer();

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
        criticalAnalyzer.CachedMethods.Should().NotBeEmpty();
    }
}