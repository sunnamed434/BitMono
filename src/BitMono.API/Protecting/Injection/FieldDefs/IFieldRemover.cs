namespace BitMono.API.Protecting.Injection.FieldDefs;

public interface IFieldRemover
{
    bool Remove(string name, ModuleDefMD moduleDefMD);
}