namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
[IL2CPPIncompatible("Changes return types to System.Object without boxing the returned value; il2cpp.exe's static IL-to-C++ converter rejects the type-unsafe return")]
public class ObjectReturnType : Protection
{
    public ObjectReturnType(IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync()
    {
        var factory = Context.Module.CorLibTypeFactory;
        var systemBoolean = factory.Boolean;
        var systemObject = factory.Object;
        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method.Signature == null)
            {
                continue;
            }
            if (!method.Signature.Returns(systemBoolean))
            {
                continue;
            }
            if (method.IsConstructor || method.IsVirtual || method.IsSetMethod || method.IsGetMethod || method.IsAsync())
            {
                continue;
            }
            if (method.ParameterDefinitions.Any(p => p.IsOut || p.IsIn))
            {
                continue;
            }

            method.Signature.ReturnType = systemObject;
        }
        return Task.CompletedTask;
    }
}