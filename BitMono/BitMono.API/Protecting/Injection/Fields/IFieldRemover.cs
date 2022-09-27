using dnlib.DotNet;

namespace BitMono.API.Protecting.Injection.Fields
{
    public interface IFieldRemover
    {
        bool Remove(string name, ModuleDefMD moduleDefMD);
    }
}