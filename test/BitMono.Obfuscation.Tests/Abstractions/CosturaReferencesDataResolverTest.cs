namespace BitMono.Obfuscation.Tests.Abstractions;

public class CosturaReferencesDataResolverTest
{
    [Fact]
    public void
        WhenCosturaReferencesDataResolving_AndModuleHasCostura_ThenCountOfReferencesShouldHaveCountAsInModule()
    {
        var resolver = new CosturaReferencesDataResolver();
        var module = ModuleDefinition.FromFile(typeof(TestCases.CosturaDecompressor.Program).Assembly.Location);
        var countOfEmbeddedCosturaResources = module.Resources.Count(r => r.IsEmbeddedCosturaResource());

        var result = resolver.Resolve(module);

        result
            .Should()
            .NotBeEmpty().And
            .HaveCount(countOfEmbeddedCosturaResources);
    }

    [Fact]
    public void WhenIsEmbeddedCosturaResource_AndResourceIsCostura_ThenShouldBeTrue()
    {
        const string EntityFrameworkCoreCosturaResourceName = "costura.microsoft.entityframeworkcore.dll.compressed";
        var module = ModuleDefinition.FromFile(typeof(TestCases.CosturaDecompressor.Program).Assembly.Location);

        var result = module.Resources.First(r => r.Name.Value.Equals(EntityFrameworkCoreCosturaResourceName));

        result.IsEmbeddedCosturaResource()
            .Should()
            .BeTrue();
    }
}