using dnlib.DotNet;

namespace BitMono.API.Protecting.Injection.MethodDefs
{
    public interface IMethodDefSearcher
    {
        MethodDef Find(string name, ModuleDefMD moduleDefMD);
        MethodDef FindInGlobalNestedMethods(string name, ModuleDefMD moduleDefMD);
    }
}