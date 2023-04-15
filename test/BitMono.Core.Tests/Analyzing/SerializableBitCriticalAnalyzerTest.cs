namespace BitMono.Core.Tests.Analyzing;

public class SerializableBitCriticalAnalyzerTest
{
    [Fact]
    public void WhenTypeSerializableBitCriticalAnalyzing_AndTypeHasSerializableBit_ThenShouldBeFalse()
    {
        var obfuscation = new ObfuscationSettings
        {
            SerializableBitObfuscationExclude = true
        };
        var criticalAnalyzer = Setup.SerializableBitCriticalAnalyzer(Options.Create(obfuscation));
        var module = ModuleDefinition.FromFile(typeof(SerializableTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(SerializableTypes));
        var type = types.NestedTypes.First(n => n.Name == nameof(SerializableTypes.SerializableBit));

        var result = criticalAnalyzer.NotCriticalToMakeChanges(type);

        result.Should().BeFalse();
    }
    [Fact]
    public void WhenTypeSerializableBitCriticalAnalyzing_AndTypeHasNoSerializableBit_ThenShouldBeTrue()
    {
        var obfuscation = new ObfuscationSettings
        {
            SerializableBitObfuscationExclude = true
        };
        var criticalAnalyzer = Setup.SerializableBitCriticalAnalyzer(Options.Create(obfuscation));
        var module = ModuleDefinition.FromFile(typeof(SerializableTypes).Assembly.Location);
        var types = module.TopLevelTypes.First(t => t.Name == nameof(SerializableTypes));
        var type = types.NestedTypes.First(n => n.Name == nameof(SerializableTypes.NoSerializableBit));
        
        var result = criticalAnalyzer.NotCriticalToMakeChanges(type);

        result.Should().BeTrue();
    }
}