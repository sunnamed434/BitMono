namespace BitMono.Core.Tests.Resolvers;

public class ObfuscationAttributeResolverTest
{
    [Theory]
    [InlineData(nameof(CallToCalli))]
    public void ShouldReturnFalse_WhenExcludeIsFalse(string feature)
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n =>
            n.Name == nameof(ObfuscationTypes.ObfuscationAttributeCallToCalliWithExcludeFalse));

        var result = resolver.Resolve(feature, type);

        result
            .ShouldBeFalse();
    }

    [Theory]
    [InlineData(nameof(CallToCalli))]
    public void ShouldReturnTrue_WhenExcludeIsTrue(string feature)
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n => n.Name == nameof(ObfuscationTypes.ObfuscationAttributeCallToCalli));

        var result = resolver.Resolve(feature, type);

        result
            .ShouldBeTrue();
    }

    [Fact]
    public void ShouldReturnFalse_WhenVoidAttributeWithEmptyFeature()
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n => n.Name == nameof(ObfuscationTypes.VoidObfuscationAttribute));

        var result = resolver.Resolve(string.Empty, type);

        result
            .ShouldBeFalse();
    }
    
    [Fact]
    public void ShouldReturnFalse_WhenMethodHasVoidAttributeWithEmptyFeature()
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(n => n.Name == nameof(ObfuscationMethods));
        var method = type.Methods.First(m => m.Name == nameof(ObfuscationMethods.VoidObfuscationAttribute));

        var result = resolver.Resolve(string.Empty, method);

        result
            .ShouldBeFalse();
    }

    [Theory]
    [InlineData(nameof(CallToCalli))]
    public void ShouldReturnTrue_WhenApplyToMembersIsFalse(string feature)
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n =>
            n.Name == nameof(ObfuscationTypes.ObfuscationAttributeCallToCalliWithApplyToMembersFalse));

        var result = resolver.Resolve(feature, type, out var attributes);

        result.ShouldBeTrue();
        attributes.ShouldNotBeNull();
        attributes!.Count.ShouldBe(1);
        attributes[0].ApplyToMembers.ShouldBeFalse();
        attributes[0].Exclude.ShouldBeTrue();
        attributes[0].Feature.ShouldBe(feature);
    }

    [Theory]
    [InlineData(nameof(CallToCalli))]
    public void ShouldReturnTrue_WhenStripAfterObfuscationIsFalse(string feature)
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n =>
            n.Name == nameof(ObfuscationTypes.ObfuscationAttributeCallToCalliWithStripAfterObfuscationFalse));

        var result = resolver.Resolve(feature, type, out var attributes);

        result.ShouldBeTrue();
        attributes.ShouldNotBeNull();
        attributes!.Count.ShouldBe(1);
        attributes[0].StripAfterObfuscation.ShouldBeFalse();
        attributes[0].Exclude.ShouldBeTrue();
        attributes[0].Feature.ShouldBe(feature);
    }

    [Theory]
    [InlineData("DifferentFeature")]
    public void ShouldReturnTrue_WhenFeatureMatches(string feature)
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n =>
            n.Name == nameof(ObfuscationTypes.ObfuscationAttributeDifferentFeature));

        var result = resolver.Resolve(feature, type);

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(nameof(CallToCalli))]
    public void ShouldReturnFalse_WhenFeatureDoesNotMatch(string feature)
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n =>
            n.Name == nameof(ObfuscationTypes.ObfuscationAttributeDifferentFeature));

        var result = resolver.Resolve(feature, type);

        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldReturnTrue_WhenEmptyFeatureMatchesEmptySearch()
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n =>
            n.Name == nameof(ObfuscationTypes.ObfuscationAttributeEmptyFeature));

        var result = resolver.Resolve(string.Empty, type);

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(nameof(CallToCalli))]
    public void ShouldReturnFalse_WhenEmptyFeatureDoesNotMatchSearch(string feature)
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n =>
            n.Name == nameof(ObfuscationTypes.ObfuscationAttributeEmptyFeature));

        var result = resolver.Resolve(feature, type);

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(nameof(CallToCalli))]
    public void ShouldReturnTrue_WhenNoFeatureSpecified(string feature)
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n =>
            n.Name == nameof(ObfuscationTypes.ObfuscationAttributeNoFeature));

        var result = resolver.Resolve(feature, type, out var attributes);

        result.ShouldBeTrue();
        attributes.ShouldNotBeNull();
        attributes!.Count.ShouldBe(1);
        attributes[0].Feature.ShouldBe("all");
        attributes[0].Exclude.ShouldBeTrue();
    }

    [Theory]
    [InlineData(nameof(CallToCalli))]
    public void ShouldReturnTrue_WhenAllPropertiesAreTrue(string feature)
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n =>
            n.Name == nameof(ObfuscationTypes.ObfuscationAttributeAllPropertiesTrue));

        var result = resolver.Resolve(feature, type, out var attributes);

        result.ShouldBeTrue();
        attributes.ShouldNotBeNull();
        attributes!.Count.ShouldBe(1);
        attributes[0].Exclude.ShouldBeTrue();
        attributes[0].ApplyToMembers.ShouldBeTrue();
        attributes[0].StripAfterObfuscation.ShouldBeTrue();
        attributes[0].Feature.ShouldBe(feature);
    }

    [Fact]
    public void ShouldReturnFalse_WhenResolverIsDisabled()
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = false,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n => n.Name == nameof(ObfuscationTypes.ObfuscationAttributeCallToCalli));

        var result = resolver.Resolve(nameof(CallToCalli), type);

        result.ShouldBeFalse();
    }

    [Fact]
    public void ShouldReturnFalse_WhenNoAttributePresent()
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(ObfuscationTypes));
        var type = types.NestedTypes.First(n => n.Name == nameof(ObfuscationTypes.NoObfuscationAttribute));

        var result = resolver.Resolve(nameof(CallToCalli), type);

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(nameof(CallToCalli))]
    public void ShouldReturnTrue_WhenMethodHasExcludeTrue(string feature)
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(n => n.Name == nameof(ObfuscationMethods));
        var method = type.Methods.First(m => m.Name == nameof(ObfuscationMethods.ObfuscationAttributeFeatureCallToCalliExcludeTrue));

        var result = resolver.Resolve(feature, method, out var attributes);

        result.ShouldBeTrue();
        attributes.ShouldNotBeNull();
        attributes!.Count.ShouldBe(1);
        attributes[0].Exclude.ShouldBeTrue();
        attributes[0].Feature.ShouldBe(feature);
    }

    [Theory]
    [InlineData(nameof(CallToCalli))]
    public void ShouldReturnFalse_WhenMethodHasExcludeFalse(string feature)
    {
        var obfuscation = new ObfuscationSettings
        {
            ObfuscationAttributeObfuscationExclude = true,
        };
        var resolver = Setup.ObfuscationAttributeResolver(obfuscation);
        var module = ModuleDefinition.FromFile(typeof(ObfuscationMethods).Assembly.Location);
        var type = module.TopLevelTypes.First(n => n.Name == nameof(ObfuscationMethods));
        var method = type.Methods.First(m => m.Name == nameof(ObfuscationMethods.ObfuscationAttributeFeatureCallToCalliExcludeFalse));

        var result = resolver.Resolve(feature, method);

        result.ShouldBeFalse();
    }
}