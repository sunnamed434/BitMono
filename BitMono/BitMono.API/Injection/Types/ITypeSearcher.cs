using dnlib.DotNet;

namespace BitMono.API.Injection.Types
{
    public interface ITypeSearcher
    {
        TypeDef Find(string name, ModuleDefMD moduleDefMD);
        TypeDef FindInGlobalNestedTypes(string name, ModuleDefMD moduleDefMD);
    }
}