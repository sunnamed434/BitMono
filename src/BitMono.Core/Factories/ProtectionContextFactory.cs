namespace BitMono.Core.Factories;

public class ProtectionContextFactory
{
    private readonly IEngineContextAccessor _engineContextAccessor;
    private readonly ProtectionParametersFactory _protectionParametersFactory;

    public ProtectionContextFactory(
        IEngineContextAccessor engineContextAccessor,
        ProtectionParametersFactory protectionParametersFactory)
    {
        _engineContextAccessor = engineContextAccessor;
        _protectionParametersFactory = protectionParametersFactory;
    }

    public ProtectionContext Create(IProtection target)
    {
        var engineContext = _engineContextAccessor.Instance;
        var protectionParameters = _protectionParametersFactory.Create(target, engineContext.Module);
        return new ProtectionContext(engineContext.Module, engineContext.RuntimeModule, engineContext.BitMonoContext,
            protectionParameters, engineContext.CancellationToken);
    }
}