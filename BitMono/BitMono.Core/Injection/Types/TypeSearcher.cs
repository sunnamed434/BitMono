using BitMono.API.Injection.Types;
using dnlib.DotNet;

namespace BitMono.Core.Injection.Types
{
    public class TypeSearcher : ITypeSearcher
    {
        public TypeDef Find(string name, ModuleDefMD moduleDefMD)
        {
            foreach (TypeDef type in moduleDefMD.Types)
            {
                if (type.Name.Equals(name))
                {
                    return type;
                }
            }
            return null;
        }
        public TypeDef FindInGlobalNestedTypes(string name, ModuleDefMD moduleDefMD)
        {
            foreach (TypeDef type in moduleDefMD.GlobalType.NestedTypes)
            {
                if (type.Name.Equals(name))
                {
                    return type;
                }
            }
            return null;
        }
    }
}