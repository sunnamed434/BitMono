using BitMono.API.Protecting.Injection.Types;
using dnlib.DotNet;
using System.Linq;

namespace BitMono.Core.Protecting.Injection.Types
{
    public class TypeSearcher : ITypeSearcher
    {
        public TypeDef Find(string name, ModuleDefMD moduleDefMD)
        {
            foreach (TypeDef typeDef in moduleDefMD.GetTypes().ToArray())
            {
                if (typeDef.Name.Equals(name))
                {
                    return typeDef;
                }
            }
            return null;
        }
        public TypeDef FindInGlobalNestedTypes(string name, ModuleDefMD moduleDefMD)
        {
            foreach (TypeDef typeDef in moduleDefMD.GlobalType.NestedTypes.ToArray())
            {
                if (typeDef.Name.Equals(name))
                {
                    return typeDef;
                }
            }
            return null;
        }
    }
}