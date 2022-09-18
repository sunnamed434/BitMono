using BitMono.API.Injection.Methods;
using dnlib.DotNet;

namespace BitMono.Core.Injection.Methods
{
    public class MethodSearcher : IMethodSearcher
    {
        public MethodDef Find(string name, ModuleDefMD moduleDefMD)
        {
            foreach (TypeDef type in moduleDefMD.Types)
            {
                if (type.HasMethods)
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.HasBody)
                        {
                            if (method.Name.Equals(name))
                            {
                                return method;
                            }
                        }
                    }
                }
            }
            return null;
        }
        public MethodDef FindInGlobalNestedMethods(string name, ModuleDefMD moduleDefMD)
        {
            foreach (TypeDef type in moduleDefMD.GlobalType.NestedTypes)
            {
                if (type.HasMethods)
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.HasBody)
                        {
                            if (method.Name.Equals(name))
                            {
                                return method;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}