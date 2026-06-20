namespace BitMono.Core.Tests.Analyzing;

public class UnityStringInvokeCriticalAnalyzerTest
{
    private static UnityStringInvokeCriticalAnalyzer CreateAnalyzer(bool enabled = true)
    {
        return Setup.UnityStringInvokeCriticalAnalyzer(new ObfuscationSettings
        {
            UnityStringInvokeMethodsObfuscationExclude = enabled
        });
    }

    private static MethodDefinition Method(string typeName, string methodName)
    {
        var module = ModuleDefinition.FromFile(typeof(UnityInvoker).Assembly.Location);
        return module.GetAllTypes().First(t => t.Name == typeName).Methods.First(m => m.Name == methodName);
    }

    [Fact]
    public void InvokeTargetIsCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Method(nameof(UnityInvoker), "DelayedSpawn")).Should().BeFalse();
    }

    [Fact]
    public void StartCoroutineTargetIsCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Method(nameof(UnityInvoker), "RunRoutine")).Should().BeFalse();
    }

    [Fact]
    public void SendMessageTargetIsCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Method(nameof(UnityInvoker), "OnPing")).Should().BeFalse();
    }

    [Fact]
    public void NonInvokedMethodIsNotCritical()
    {
        CreateAnalyzer().NotCriticalToMakeChanges(Method(nameof(UnityInvoker), "NotInvokedByString")).Should().BeTrue();
    }

    [Fact]
    public void NothingIsCriticalWhenDisabled()
    {
        CreateAnalyzer(enabled: false).NotCriticalToMakeChanges(Method(nameof(UnityInvoker), "DelayedSpawn")).Should().BeTrue();
    }
}
