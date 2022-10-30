using dnlib.DotNet;

namespace BitMono.API.Protecting.Injection.TypeDefs
{
    public interface ITypeDefSearcher
    {
        TypeDef Find(string name, ModuleDefMD moduleDefMD);
        TypeDef FindInGlobalNestedTypes(string name, ModuleDefMD moduleDefMD);
    }
}