using dnlib.DotNet;

namespace BitMono.Obfuscation
{
    public class ModuleCreationOptionsCreator
    {
        public ModuleCreationOptions Create(ModuleContext moduleContext)
        {
            return new ModuleCreationOptions(moduleContext, CLRRuntimeReaderKind.Mono);
        }
    }
}