namespace BitMono.API.Protecting.Contexts;

public class ProtectionContext
{
    [AllowNull] public ModuleDefMD ModuleDefMD { get; set; }
    [AllowNull] public ModuleCreationOptions ModuleCreationOptions { get; set; }
    [AllowNull] public ModuleWriterOptions ModuleWriterOptions { get; set; }
    [AllowNull] public ModuleDefMD RuntimeModuleDefMD { get; set; }
    [AllowNull] public Importer Importer { get; set; }
    [AllowNull] public Importer RuntimeImporter { get; set; }
    [AllowNull] public BitMonoContext BitMonoContext { get; set; }
    [AllowNull] public byte[] ModuleDefMDOutput { get; set; }

    [AllowNull] public ModuleContext ModuleContext => ModuleCreationOptions.Context;
    [AllowNull] public AssemblyResolver AssemblyResolver => (AssemblyResolver)ModuleContext.AssemblyResolver;
}