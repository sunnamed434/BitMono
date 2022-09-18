using BitMono.API.Injection.Types;
using dnlib.DotNet;

namespace BitMono.Core.Injection.Types
{
    public class TypeRemover : ITypeRemover
    {
        public bool Remove(string name, ModuleDefMD module)
        {
            foreach (TypeDef type in module.Types)
            {
                if (type.Name.Equals(name))
                {
                    module.Types.Remove(type);
                    return true;
                }
            }
            return false;
        }
    }
}