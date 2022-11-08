using dnlib.DotNet;
using dnlib.DotNet.Writer;
using NullGuard;

namespace BitMono.API.Protecting.Context
{
    public class ProtectionContext
    {
        [AllowNull] public ModuleDefMD ModuleDefMD { get; set; }
        [AllowNull] public ModuleCreationOptions ModuleCreationOptions { get; set; }
        [AllowNull] public ModuleWriterOptions ModuleWriterOptions { get; set; }
        [AllowNull] public ModuleDefMD ExternalComponentsModuleDefMD { get; set; }
        [AllowNull] public Importer Importer { get; set; }
        [AllowNull] public Importer ExternalComponentsImporter { get; set; }
        [AllowNull] public BitMonoContext BitMonoContext { get; set; }

        [AllowNull] public ModuleContext ModuleContext => ModuleCreationOptions.Context;
        [AllowNull] public AssemblyResolver AssemblyResolver => (AssemblyResolver)ModuleContext.AssemblyResolver;
    }
}