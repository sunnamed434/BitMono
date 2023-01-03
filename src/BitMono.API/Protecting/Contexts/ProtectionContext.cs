namespace BitMono.API.Protecting.Contexts;

public class ProtectionContext
{
    [AllowNull] public ModuleDefinition Module { get; set; }
    [AllowNull] public ModuleReaderParameters ModuleReaderParameters { get; set; }
    [AllowNull] public IPEImageBuilder PEImageBuilder { get; set; }
    [AllowNull] public ModuleDefinition RuntimeModule { get; set; }
    [AllowNull] public ReferenceImporter RuntimeImporter { get; set; }
    [AllowNull] public BitMonoContext BitMonoContext { get; set; }
    [AllowNull] public byte[] ModuleOutput { get; set; }
    [AllowNull] public CancellationToken CancellationToken { get; set; }

    [AllowNull] public IAssemblyResolver AssemblyResolver => Module.MetadataResolver.AssemblyResolver;
    [AllowNull] public ReferenceImporter Importer => Module.DefaultImporter;

    public void ThrowIfCancellationRequested()
    {
        CancellationToken.ThrowIfCancellationRequested();
    }
}