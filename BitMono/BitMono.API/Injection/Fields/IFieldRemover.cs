using dnlib.DotNet;

namespace BitMono.API.Injection.Fields
{
    public interface IFieldRemover
    {
        bool Remove(string name, ModuleDefMD moduleDefMD);
    }
}