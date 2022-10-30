using dnlib.DotNet;

namespace BitMono.API.Protecting.Injection.FieldDefs
{
    public interface IFieldSearcher
    {
        FieldDef Find(string name, ModuleDefMD moduleDefMD);
        FieldDef FindInGlobalNestedTypes(string name, ModuleDefMD moduleDefMD);
    }
}