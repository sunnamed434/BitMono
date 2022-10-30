using BitMono.API.Protecting.Injection.MethodDefs;
using dnlib.DotNet;
using NullGuard;

namespace BitMono.Core.Protecting.Injection.MethodDefs
{
    public class MethodDefsSearcher : IMethodDefSearcher
    {
        [return: AllowNull]
        public MethodDef Find(string name, ModuleDefMD moduleDefMD)
        {
            foreach (var typeDef in moduleDefMD.Types)
            {
                foreach (var methodDef in typeDef.Methods)
                {
                    if (methodDef.Name.Equals(name))
                    {
                        return methodDef;
                    }
                }
            }
            return null;
        }
        [return: AllowNull]
        public MethodDef FindInGlobalNestedMethods(string name, ModuleDefMD moduleDefMD)
        {
            foreach (var typeDef in moduleDefMD.GlobalType.NestedTypes)
            {
                if (typeDef.HasMethods)
                {
                    foreach (var methodDef in typeDef.Methods)
                    {
                        if (methodDef.Name.Equals(name))
                        {
                            return methodDef;
                        }
                    }
                }
            }
            return null;
        }
    }
}