namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class ObjectReturnType : IProtection
{
    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        var systemBoolean = context.Module.CorLibTypeFactory.Boolean;
        var systemObject = context.Module.CorLibTypeFactory.Object;
        foreach (var method in parameters.Members.OfType<MethodDefinition>())
        {
            if (method.Signature.ReturnsValue(systemBoolean))
            {
                if (method.IsConstructor == false && method.IsVirtual == false && method.NotAsync()
                    && method.IsSetMethod == false && method.IsGetMethod == false)
                    if (method.ParameterDefinitions.Any(p => p.IsOut || p.IsIn) == false)
                    {
                        method.Signature.ReturnType = systemObject;
                    }
            }
        }
        return Task.CompletedTask;
    }
}