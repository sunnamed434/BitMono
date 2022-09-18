using BitMono.API.Injection.Methods;
using dnlib.DotNet;

namespace BitMono.Core.Injection.Methods
{
    public class MethodRemover : IMethodRemover
    {
        public bool Remove(string name, ModuleDefMD moduleDefMD)
        {
            foreach (TypeDef type in moduleDefMD.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (type.Name.Equals(name))
                    {
                        type.Methods.Remove(method);
                        return true;
                    }
                }
            }
            return false;
        }
        public bool Remove(TypeDef typeDef, string name)
        {
            foreach (var method in typeDef.Methods)
            {
                if (method.Name.Equals(name))
                {
                    typeDef.Methods.Remove(method);
                    return true;
                }
            }

            return false;
        }
    }
}