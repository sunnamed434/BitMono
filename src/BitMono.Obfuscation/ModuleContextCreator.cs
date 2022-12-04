using dnlib.DotNet;

namespace BitMono.Obfuscation
{
    public class ModuleContextCreator
    {
        public ModuleContext Create()
        {
            return ModuleDefMD.CreateModuleContext();
        }
    }
}