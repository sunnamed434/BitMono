using dnlib.DotNet;

namespace BitMono.API.Injection.Methods
{
    public interface IMethodSearcher
    {
        MethodDef Find(string name, ModuleDefMD moduleDefMD);
        MethodDef FindInGlobalNestedMethods(string name, ModuleDefMD moduleDefMD);
    }
}