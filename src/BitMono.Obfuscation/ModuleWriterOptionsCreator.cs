namespace BitMono.Obfuscation;

public class ModuleWriterOptionsCreator
{
    public ModuleWriterOptions Create(ModuleDefMD moduleDefMD)
    {
        var moduleWriterOptions = new ModuleWriterOptions(moduleDefMD);
        moduleWriterOptions.MetadataLogger = DummyLogger.NoThrowInstance;
        moduleWriterOptions.MetadataOptions.Flags |= 
              MetadataFlags.PreserveStringsOffsets 
            | MetadataFlags.PreserveUSOffsets 
            | MetadataFlags.PreserveBlobOffsets 
            | MetadataFlags.PreserveExtraSignatureData;
        moduleWriterOptions.Cor20HeaderOptions.Flags = ComImageFlags.ILOnly;
        moduleWriterOptions.PEHeadersOptions.CopyPEHeaders(moduleDefMD);
        return moduleWriterOptions;
    }
}