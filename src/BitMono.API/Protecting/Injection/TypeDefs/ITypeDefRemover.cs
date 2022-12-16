namespace BitMono.API.Protecting.Injection.TypeDefs;

public interface ITypeDefRemover
{
    bool Remove(string name, ModuleDefMD moduleDefMD);
}