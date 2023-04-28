namespace BitMono.Core.Contexts;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class StarterContext
{
#pragma warning disable CS8618
    public ModuleDefinition Module { get; set; }
    public ModuleReaderParameters ModuleReaderParameters { get; set; }
    public IPEImageBuilder PEImageBuilder { get; set; }
    public ModuleDefinition RuntimeModule { get; set; }
    public ReferenceImporter RuntimeImporter { get; set; }
    public BitMonoContext BitMonoContext { get; set; }
    public CancellationToken CancellationToken { get; set; }

    public IAssemblyResolver AssemblyResolver => Module.MetadataResolver.AssemblyResolver;
#pragma warning restore CS8618

    public void ThrowIfCancellationRequested()
    {
        CancellationToken.ThrowIfCancellationRequested();
    }
}