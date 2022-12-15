namespace BitMono.API.Protecting.Injection.MethodDefs;

public interface IMethodDefRemover
{
    bool Remove(ModuleDefMD moduleDefMD, string name);
    bool Remove(TypeDef typeDef, string name);
}