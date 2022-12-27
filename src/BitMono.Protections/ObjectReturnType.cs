namespace BitMono.Protections;

public class ObjectReturnType : IProtection
{
    private readonly RuntimeCriticalAnalyzer m_RuntimeCriticalAnalyzer;

    public ObjectReturnType(RuntimeCriticalAnalyzer runtimeCriticalAnalyzer)
    {
        m_RuntimeCriticalAnalyzer = runtimeCriticalAnalyzer;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        var boolean = context.Module.CorLibTypeFactory.Boolean;
        var @object = context.Module.CorLibTypeFactory.Object;
        foreach (var method in parameters.Targets.OfType<MethodDefinition>())
        {
            if (method.Signature.ReturnsValue && method.Signature.ReturnType != boolean)
            {
                if (method.IsConstructor == false && method.IsVirtual == false
                    && m_RuntimeCriticalAnalyzer.NotCriticalToMakeChanges(method)
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