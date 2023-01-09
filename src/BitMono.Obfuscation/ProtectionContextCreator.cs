namespace BitMono.Obfuscation;

public class ProtectionContextCreator
{
    public ModuleCreationResult ModuleCreationResult { get; set; }
    public ModuleDefinition RuntimeModuleDefinition { get; set; }
    public BitMonoContext BitMonoContext { get; set; }
    public CancellationToken CancellationToken { get; set; }

    public ProtectionContext Create()
    {
        return new ProtectionContext
        {
            Module = ModuleCreationResult.Module,
            RuntimeModule = RuntimeModuleDefinition,
            ModuleReaderParameters = ModuleCreationResult.ModuleReaderParameters,
            PEImageBuilder = ModuleCreationResult.PEImageBuilder,
            RuntimeImporter = new ReferenceImporter(RuntimeModuleDefinition),
            BitMonoContext = BitMonoContext,
            CancellationToken = CancellationToken,
        };
    }
}