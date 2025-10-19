namespace BitMono.Core.Tests.Analyzing;

public class ArgumentTracerTest
{
    private static ArgumentTracer CreateTracer()
    {
        return new ArgumentTracer();
    }

    private static (ModuleDefinition module, TypeDefinition type) GetTestData()
    {
        var module = ModuleDefinition.FromFile(typeof(ArgumentTracerTestCases).Assembly.Location);
        var type = module.TopLevelTypes.First(t => t.Name == nameof(ArgumentTracerTestCases));
        return (module, type);
    }

    [Fact]
    public void ShouldTraceDirectStringArgument()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ArgumentTracerTestCases.DirectStringArgument));
        var tracer = CreateTracer();

        var callInstruction = method.CilMethodBody!.Instructions.FirstOrDefault(i => 
            i.OpCode.Code == CilCode.Call && 
            i.Operand is IMethodDescriptor md && 
            md.Name == "GetMethod");
        callInstruction.Should().NotBeNull();

        var result = tracer.TraceStringArgument(method.CilMethodBody!, callInstruction!, 0);

        result.Should().Be("TestMethod");
    }

    [Fact]
    public void ShouldTraceVariableStringArgument()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ArgumentTracerTestCases.VariableStringArgument));
        var tracer = CreateTracer();

        var callInstruction = method.CilMethodBody!.Instructions.FirstOrDefault(i => 
            i.OpCode.Code == CilCode.Call && 
            i.Operand is IMethodDescriptor md && 
            md.Name == "GetMethod");
        callInstruction.Should().NotBeNull();

        var result = tracer.TraceStringArgument(method.CilMethodBody!, callInstruction!, 0);

        result.Should().Be("TestMethod");
    }

    [Fact]
    public void ShouldTraceMultipleArguments()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ArgumentTracerTestCases.MultipleArguments));
        var tracer = CreateTracer();

        var callInstruction = method.CilMethodBody!.Instructions.FirstOrDefault(i => 
            i.OpCode.Code == CilCode.Call && 
            i.Operand is IMethodDescriptor md && 
            md.Name == "GetMethod");
        callInstruction.Should().NotBeNull();

        var argumentIndices = tracer.TraceArguments(method.CilMethodBody!, callInstruction!);

        argumentIndices.Should().NotBeNull();
        argumentIndices.Should().HaveCount(2);
    }

    [Fact]
    public void ShouldTraceTypeArgument()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ArgumentTracerTestCases.TypeArgument));
        var tracer = CreateTracer();

        var callInstruction = method.CilMethodBody!.Instructions.FirstOrDefault(i => 
            i.OpCode.Code == CilCode.Call && 
            i.Operand is IMethodDescriptor md && 
            md.Name == "GetTypeFromHandle");
        callInstruction.Should().NotBeNull();

        var result = tracer.TraceTypeArgument(method.CilMethodBody!, callInstruction!, 0);

        result.Should().NotBeNull();
        result!.Name.Value.Should().Be(nameof(ArgumentTracerTestCases));
    }

    [Fact]
    public void ShouldTraceComplexVariableChain()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ArgumentTracerTestCases.ComplexVariableChain));
        var tracer = CreateTracer();

        var callInstruction = method.CilMethodBody!.Instructions.FirstOrDefault(i => 
            i.OpCode.Code == CilCode.Call && 
            i.Operand is IMethodDescriptor md && 
            md.Name == "GetMethod");
        callInstruction.Should().NotBeNull();

        var result = tracer.TraceStringArgument(method.CilMethodBody!, callInstruction!, 0);

        result.Should().Be("ComplexTest");
    }

    [Fact]
    public void ShouldReturnNullForInvalidCallInstruction()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ArgumentTracerTestCases.DirectStringArgument));
        var tracer = CreateTracer();

        var nonCallInstruction = method.CilMethodBody!.Instructions.First(i => i.OpCode != CilOpCodes.Call);
        var result = tracer.TraceArguments(method.CilMethodBody!, nonCallInstruction);

        result.Should().BeNull();
    }

    [Fact]
    public void ShouldReturnNullForInvalidArgumentIndex()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ArgumentTracerTestCases.DirectStringArgument));
        var tracer = CreateTracer();

        var callInstruction = method.CilMethodBody!.Instructions.FirstOrDefault(i => 
            i.OpCode.Code == CilCode.Call && 
            i.Operand is IMethodDescriptor md && 
            md.Name == "GetMethod");
        callInstruction.Should().NotBeNull();

        var result = tracer.TraceStringArgument(method.CilMethodBody!, callInstruction!, 10);

        result.Should().BeNull();
    }

    [Fact]
    public void ShouldHandleNoArguments()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ArgumentTracerTestCases.NoArguments));
        var tracer = CreateTracer();

        var callInstruction = method.CilMethodBody!.Instructions.FirstOrDefault(i => 
            i.OpCode.Code == CilCode.Call && 
            i.Operand is IMethodDescriptor md && 
            md.Name == "GetType");
        callInstruction.Should().NotBeNull();

        var result = tracer.TraceArguments(method.CilMethodBody!, callInstruction!);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldTraceArgumentsThroughMethodCall()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ArgumentTracerTestCases.ArgumentsThroughMethodCall));
        var tracer = CreateTracer();

        var callInstruction = method.CilMethodBody!.Instructions.FirstOrDefault(i => 
            i.OpCode.Code == CilCode.Call && 
            i.Operand is IMethodDescriptor md && 
            md.Name == "GetMethod");
        callInstruction.Should().NotBeNull();

        var result = tracer.TraceStringArgument(method.CilMethodBody!, callInstruction!, 0);

        result.Should().Be("MethodCallTest");
    }

    [Fact]
    public void ShouldHandleNullMethodBody()
    {
        var tracer = CreateTracer();
        var dummyInstruction = new CilInstruction(CilOpCodes.Call, null);

        var result = tracer.TraceArguments(null!, dummyInstruction);

        result.Should().BeNull();
    }

    [Fact]
    public void ShouldHandleNullCallInstruction()
    {
        var (_, type) = GetTestData();
        var method = type.Methods.First(m => m.Name == nameof(ArgumentTracerTestCases.DirectStringArgument));
        var tracer = CreateTracer();

        var result = tracer.TraceArguments(method.CilMethodBody!, null!);

        result.Should().BeNull();
    }
}

/// <summary>
/// Test cases for ArgumentTracer functionality
/// </summary>
public class ArgumentTracerTestCases
{
    public void DirectStringArgument()
    {
        _ = typeof(ArgumentTracerTestCases).GetMethod("TestMethod");
    }

    public void VariableStringArgument()
    {
        string methodName = "TestMethod";
        _ = typeof(ArgumentTracerTestCases).GetMethod(methodName);
    }

    public void MultipleArguments()
    {
        _ = typeof(ArgumentTracerTestCases).GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Instance);
    }

    public void TypeArgument()
    {
        _ = Type.GetTypeFromHandle(typeof(ArgumentTracerTestCases).TypeHandle);
    }

    public void ComplexVariableChain()
    {
        string baseName = "Complex";
        string suffix = "Test";
        string methodName = baseName + suffix;
        _ = typeof(ArgumentTracerTestCases).GetMethod(methodName);
    }

    public void NoArguments()
    {
        _ = typeof(ArgumentTracerTestCases).GetType();
    }

    public void ArgumentsThroughMethodCall()
    {
        string methodName = GetMethodName();
        _ = typeof(ArgumentTracerTestCases).GetMethod(methodName);
    }

    private static string GetMethodName()
    {
        return "MethodCallTest";
    }
}
