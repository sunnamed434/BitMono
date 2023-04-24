namespace BitMono.Protections;

[UsedImplicitly]
[DoNotResolve(MemberInclusionFlags.SpecialRuntime)]
public class ObjectReturnType : Protection
{
    public ObjectReturnType(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    public override Task ExecuteAsync()
    {
        var factory = Context.Module.CorLibTypeFactory;
        var systemBoolean = factory.Boolean;
        var systemObject = factory.Object;
        foreach (var method in Context.Parameters.Members.OfType<MethodDefinition>())
        {
            if (method.Signature != null && method.Signature.ReturnsValueOf(systemBoolean))
            {
                if (method is { IsConstructor: false, IsVirtual: false, IsSetMethod: false, IsGetMethod: false }
                    && method.NotAsync())
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