using dnlib.DotNet;

namespace BitMono.API.Protecting.Injection.Types
{
    public interface ITypeRemover
    {
        bool Remove(string name, ModuleDefMD moduleDefMD);
    }
}