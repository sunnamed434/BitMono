using dnlib.DotNet;

namespace BitMono.API.Protecting.Injection.Methods
{
    public interface IMethodRemover
    {
        bool Remove(string name, ModuleDefMD moduleDefMD);
        bool Remove(TypeDef typeDef, string name);
    }
}