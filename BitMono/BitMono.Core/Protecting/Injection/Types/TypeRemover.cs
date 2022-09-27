using BitMono.API.Protecting.Injection.Types;
using dnlib.DotNet;
using System.Linq;

namespace BitMono.Core.Protecting.Injection.Types
{
    public class TypeRemover : ITypeRemover
    {
        public bool Remove(string name, ModuleDefMD module)
        {
            foreach (TypeDef typeDef in module.GetTypes().ToArray())
            {
                if (typeDef.Name.Equals(name))
                {
                    if (typeDef.IsNested == false)
                    {
                        module.Types.Remove(typeDef);
                    }
                    else
                    {
                        typeDef.NestedTypes.Remove(typeDef);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}