namespace BitMono.Protections;

[DoNotResolve(Members.SpecialRuntime)]
public class ObjectReturnType : IProtection
{
    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        var boolean = context.Module.CorLibTypeFactory.Boolean;
        var @object = context.Module.CorLibTypeFactory.Object;
        foreach (var method in parameters.Targets.OfType<MethodDefinition>())
        {
            if (method.Signature.ReturnsValue && method.Signature.ReturnType != boolean)
            {
                if (method.IsConstructor == false && method.IsVirtual == false
                    && method.NotAsync())
                {
                    if (method.IsSetMethod == false && method.IsGetMethod == false)
                    {
                        if (method.ParameterDefinitions.Any(p => p.IsOut || p.IsIn) == false)
                        {
                            method.Signature.ReturnType = @object;
                        }
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}