namespace BitMono.Core.Tests.Analyzing;

public class ReflectionCriticalAnalyzerTest
{
    private static ReflectionCriticalAnalyzer CreateAnalyzer(bool reflectionEnabled = true)
    {
        return Setup.ReflectionCriticalAnalyzer(new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = reflectionEnabled
        });
    }

    private static ModuleDefinition GetModule()
    {
        return ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
    }

    private static TypeDefinition Type(ModuleDefinition module, string name)
    {
        return module.GetAllTypes().First(t => t.Name == name);
    }

    private static MethodDefinition Method(TypeDefinition type, string name)
    {
        return type.Methods.First(m => m.Name == name);
    }

    // --- targets are excluded -------------------------------------------------------------------

    [Fact]
    public void ReflectedMethod_IsExcluded()
    {
        var type = Type(GetModule(), nameof(ReflectionMethods));
        var analyzer = CreateAnalyzer();

        analyzer.NotCriticalToMakeChanges(Method(type, nameof(ReflectionMethods.UsesReflectionOnItSelf)))
            .Should().BeFalse();
    }

    [Fact]
    public void ReflectedField_IsExcluded()
    {
        var type = Type(GetModule(), nameof(ReflectionMethods));
        var field = type.Fields.First(f => f.Name == nameof(ReflectionMethods.TestField));

        CreateAnalyzer().NotCriticalToMakeChanges(field).Should().BeFalse();
    }

    [Fact]
    public void ReflectedProperty_IsExcluded()
    {
        var type = Type(GetModule(), nameof(ReflectionMethods));
        var property = type.Properties.First(p => p.Name == nameof(ReflectionMethods.TestProperty));

        CreateAnalyzer().NotCriticalToMakeChanges(property).Should().BeFalse();
    }

    [Fact]
    public void ReflectedEvent_IsExcluded()
    {
        var type = Type(GetModule(), nameof(ReflectionMethods));
        var eventDef = type.Events.First(e => e.Name == nameof(ReflectionMethods.TestEvent));

        CreateAnalyzer().NotCriticalToMakeChanges(eventDef).Should().BeFalse();
    }

    // --- the over-exclusion fixes (the point of the rewrite) ------------------------------------

    [Fact]
    public void MethodThatReflectsOnlyOnOthers_IsNotExcluded()
    {
        // ReflectsOnOthers reflects on ProbeReflected.Shared and a field - never on itself, so renaming
        // the caller can't break anything. The old analyzer froze every reflection-using method.
        var type = Type(GetModule(), nameof(ReflectionApiCases));

        CreateAnalyzer().NotCriticalToMakeChanges(Method(type, nameof(ReflectionApiCases.ReflectsOnOthers)))
            .Should().BeTrue();
    }

    [Fact]
    public void SameNamedMemberOnUnrelatedType_StaysObfuscatable()
    {
        // Only ProbeReflected.Shared is reflected. The identically named ProbeUntouched.Shared must
        // stay obfuscatable - the old name-only matcher froze both.
        var module = GetModule();
        var reflected = Method(Type(module, nameof(ProbeReflected)), nameof(ProbeReflected.Shared));
        var untouched = Method(Type(module, nameof(ProbeUntouched)), nameof(ProbeUntouched.Shared));
        var analyzer = CreateAnalyzer();

        analyzer.NotCriticalToMakeChanges(reflected).Should().BeFalse();
        analyzer.NotCriticalToMakeChanges(untouched).Should().BeTrue();
    }

    [Fact]
    public void BareTypeof_DoesNotExcludeTheType()
    {
        // typeof(X) with no name-based lookup must not freeze X (it used to).
        var type = Type(GetModule(), nameof(BareTypeofProbe));

        CreateAnalyzer().NotCriticalToMakeChanges(type).Should().BeTrue();
    }

    [Fact]
    public void NonReflectedMember_IsNotExcluded()
    {
        var type = Type(GetModule(), nameof(ReflectionApiCases));

        CreateAnalyzer().NotCriticalToMakeChanges(Method(type, nameof(ReflectionApiCases.Untouched)))
            .Should().BeTrue();
    }

    // --- type-by-name ---------------------------------------------------------------------------

    [Fact]
    public void GetTypeByName_ExcludesTheType()
    {
        var type = Type(GetModule(), nameof(GetTypeProbe));

        CreateAnalyzer().NotCriticalToMakeChanges(type).Should().BeFalse();
    }

    // --- newly covered reflection APIs ----------------------------------------------------------

    [Fact]
    public void GetRuntimeMethod_ExcludesTarget()
    {
        var type = Type(GetModule(), nameof(ReflectionApiCases));

        CreateAnalyzer().NotCriticalToMakeChanges(Method(type, nameof(ReflectionApiCases.Target)))
            .Should().BeFalse();
    }

    [Fact]
    public void GetRuntimeProperty_ExcludesTarget()
    {
        var type = Type(GetModule(), nameof(ReflectionApiCases));
        var property = type.Properties.First(p => p.Name == nameof(ReflectionApiCases.ApiProperty));

        CreateAnalyzer().NotCriticalToMakeChanges(property).Should().BeFalse();
    }

    [Fact]
    public void CreateDelegateByName_ExcludesTarget()
    {
        var type = Type(GetModule(), nameof(ReflectionApiCases));

        CreateAnalyzer().NotCriticalToMakeChanges(Method(type, nameof(ReflectionApiCases.DelegateTarget)))
            .Should().BeFalse();
    }

    [Fact]
    public void EnumParse_ExcludesEnumFieldsButNotTheEnumType()
    {
        var module = GetModule();
        var color = Type(module, nameof(Color));
        var red = color.Fields.First(f => f.Name == nameof(Color.Red));
        var analyzer = CreateAnalyzer();

        analyzer.NotCriticalToMakeChanges(red).Should().BeFalse();
        analyzer.NotCriticalToMakeChanges(color).Should().BeTrue();
    }

    [Fact]
    public void GetNestedType_ExcludesNested()
    {
        var nested = Type(GetModule(), nameof(ReflectionApiCases.Nested));

        CreateAnalyzer().NotCriticalToMakeChanges(nested).Should().BeFalse();
    }

    [Fact]
    public void ReflectedOverride_FreezesBaseDeclaration()
    {
        var module = GetModule();
        var derived = Method(Type(module, nameof(DerivedProbe)), nameof(DerivedProbe.Virt));
        var baseMethod = Method(Type(module, nameof(BaseProbe)), nameof(BaseProbe.Virt));
        var analyzer = CreateAnalyzer();

        analyzer.NotCriticalToMakeChanges(derived).Should().BeFalse();
        analyzer.NotCriticalToMakeChanges(baseMethod).Should().BeFalse();
    }

    [Fact]
    public void PluralGetMethods_ExcludesEveryMethodOfTheResolvedType()
    {
        // ReflectionMethods.UsesGetMethods does typeof(ReflectionMethods).GetMethods().
        var type = Type(GetModule(), nameof(ReflectionMethods));

        CreateAnalyzer().NotCriticalToMakeChanges(Method(type, nameof(ReflectionMethods.VoidMethod)))
            .Should().BeFalse();
    }

    // --- GetILAsByteArray -> method-body critical -----------------------------------------------

    [Fact]
    public void ReadingIlViaReflection_MarksTargetMethodBodyCritical()
    {
        var type = Type(GetModule(), nameof(ReflectionApiCases));

        CreateAnalyzer().IsMethodBodyCritical(Method(type, nameof(ReflectionApiCases.IlTarget)))
            .Should().BeTrue();
    }

    [Fact]
    public void MethodBodyCritical_IsFalseForMethodsWhoseIlIsNotRead()
    {
        // Target is rename-excluded (GetRuntimeMethod) but its IL is never read - IL rewriters may touch it.
        var type = Type(GetModule(), nameof(ReflectionApiCases));

        CreateAnalyzer().IsMethodBodyCritical(Method(type, nameof(ReflectionApiCases.Target)))
            .Should().BeFalse();
    }

    // --- setting toggle -------------------------------------------------------------------------

    [Fact]
    public void WhenReflectionExcludeDisabled_NothingIsCritical()
    {
        var module = GetModule();
        var type = Type(module, nameof(ReflectionMethods));
        var method = Method(type, nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var field = type.Fields.First(f => f.Name == nameof(ReflectionMethods.TestField));
        var analyzer = CreateAnalyzer(reflectionEnabled: false);

        analyzer.NotCriticalToMakeChanges(method).Should().BeTrue();
        analyzer.NotCriticalToMakeChanges(field).Should().BeTrue();
        analyzer.IsMethodBodyCritical(Method(Type(module, nameof(ReflectionApiCases)), nameof(ReflectionApiCases.IlTarget)))
            .Should().BeFalse();
    }

    [Fact]
    public void RepeatedQueries_AreStable()
    {
        var type = Type(GetModule(), nameof(ReflectionMethods));
        var method = Method(type, nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var analyzer = CreateAnalyzer();

        analyzer.NotCriticalToMakeChanges(method).Should().BeFalse();
        analyzer.NotCriticalToMakeChanges(method).Should().BeFalse();
        analyzer.CachedMethods.Count(m => m == method).Should().Be(1);
    }
}
