namespace BitMono.Core.Contexts;

public class ProtectionContext
{
    public ProtectionContext(ModuleDefinition module, ModuleDefinition runtimeModule, BitMonoContext bitMonoContext,
        ProtectionParameters parameters, CancellationToken cancellationToken)
    {
        Module = module;
        RuntimeModule = runtimeModule;
        BitMonoContext = bitMonoContext;
        Parameters = parameters;
        CancellationToken = cancellationToken;
    }

    public ModuleDefinition Module { get; }
    public ModuleDefinition RuntimeModule { get; }
    public BitMonoContext BitMonoContext { get; }
    public ProtectionParameters Parameters { get; }
    public CancellationToken CancellationToken { get; }

    public ReferenceImporter ModuleImporter => Module.DefaultImporter;
    public ReferenceImporter RuntimeImporter => Module.DefaultImporter;

    public void ThrowIfCancellationTokenRequested()
    {
        CancellationToken.ThrowIfCancellationRequested();
    }
}