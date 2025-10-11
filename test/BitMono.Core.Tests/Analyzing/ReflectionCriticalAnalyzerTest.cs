namespace BitMono.Core.Tests.Analyzing;

public class ReflectionCriticalAnalyzerTest
{
    [Fact]
    public void ShouldDetectSelfReflection()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectMultipleReflectionCalls()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.Uses3Reflection));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectFieldReflection()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesFieldReflection));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectPrivateFieldReflection()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesPrivateFieldReflection));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectPropertyReflection()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesPropertyReflection));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectReadOnlyPropertyReflection()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReadOnlyPropertyReflection));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectEventReflection()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesEventReflection));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectGetMemberForMethod()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMemberForMethod));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectGetMemberForField()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMemberForField));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectGetMemberForProperty()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMemberForProperty));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectGetMemberForEvent()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesGetMemberForEvent));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectVariableForMethodReflection()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesVariableForMethodReflection));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectVariableForFieldReflection()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesVariableForFieldReflection));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectVariableForPropertyReflection()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesVariableForPropertyReflection));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectVariableForEventReflection()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesVariableForEventReflection));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectTypeGetTypeFromHandle()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesTypeGetTypeFromHandle));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectLdtokenForType()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesLdtokenForType));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldDetectMultipleReflectionTypes()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesAllReflectionTypes));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldDetectReflectionWithBindingFlags()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionWithBindingFlags));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldNotDetectReflectionWhenDisabled()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesReflectionOnItSelf));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = false
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldNotDetectReflectionInNonReflectionMethod()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.VoidMethod));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldDetectComplexReflectionPatterns()
    {
        var module = ModuleDefinition.FromFile(typeof(ReflectionMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ReflectionMethods));
        var method = type.Methods.First(m => m.Name == nameof(ReflectionMethods.UsesComplexReflectionPatterns));
        var obfuscation = new ObfuscationSettings
        {
            ReflectionMembersObfuscationExclude = true
        };
        var criticalAnalyzer = new ReflectionCriticalAnalyzer(Options.Create(obfuscation));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(method);

        result.Should().BeFalse();
    }
}