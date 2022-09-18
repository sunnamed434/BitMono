using dnlib.DotNet;

namespace BitMono.API.Injection.Methods
{
    public interface IMethodRemover
    {
        bool Remove(string name, ModuleDefMD moduleDefMD);
        bool Remove(TypeDef typeDef, string name);
    }
}