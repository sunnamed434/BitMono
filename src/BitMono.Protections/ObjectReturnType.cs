namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class ObjectReturnType : Protection
{
    public ObjectReturnType(ProtectionContext context) : base(context)
    {
    }

    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        var factory = Context.Module.CorLibTypeFactory;
        var systemBoolean = factory.Boolean;
        var systemObject = factory.Object;
        foreach (var method in parameters.Members.OfType<MethodDefinition>())
        {
            if (method.Signature.ReturnsValue(systemBoolean))
            {
                if (method.IsConstructor == false && method.IsVirtual == false && method.NotAsync()
                    && method.IsSetMethod == false && method.IsGetMethod == false)
                {
                    if (method.ParameterDefinitions.Any(p => p.IsOut || p.IsIn) == false)
                    {
                        method.Signature.ReturnType = systemObject;
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}