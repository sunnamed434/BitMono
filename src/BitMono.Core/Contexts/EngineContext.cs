namespace BitMono.Core.Contexts;

public class EngineContext
{
    public ModuleDefinition Module { get; set; }
    public ModuleReaderParameters ModuleReaderParameters { get; set; }
    public IPEImageBuilder PEImageBuilder { get; set; }
    public ModuleDefinition RuntimeModule { get; set; }
    public ReferenceImporter RuntimeImporter { get; set; }
    public BitMonoContext BitMonoContext { get; set; }
    public CancellationToken CancellationToken { get; set; }

    public IAssemblyResolver AssemblyResolver => Module?.MetadataResolver.AssemblyResolver;

    public void ThrowIfCancellationRequested()
    {
        CancellationToken.ThrowIfCancellationRequested();
    }
}