using dnlib.DotNet;

namespace BitMono.API.Injection.Types
{
    public interface ITypeRemover
    {
        bool Remove(string name, ModuleDefMD moduleDefMD);
    }
}