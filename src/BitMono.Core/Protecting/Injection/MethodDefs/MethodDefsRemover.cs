namespace BitMono.Core.Protecting.Injection.MethodDefs;

public class MethodDefsRemover : IMethodDefRemover
{
    public bool Remove(ModuleDefMD moduleDefMD, string name)
    {
        foreach (var typeDef in moduleDefMD.GetTypes().ToArray())
        {
            foreach (var methodDef in typeDef.Methods)
            {
                if (typeDef.Name.Equals(name))
                {
                    typeDef.Methods.Remove(methodDef);
                    return true;
                }
            }
        }
        return false;
    }
    public bool Remove(TypeDef typeDef, string name)
    {
        foreach (var methodDef in typeDef.Methods.ToArray())
        {
            if (methodDef.Name.Equals(name))
            {
                typeDef.Methods.Remove(methodDef);
                return true;
            }
        }
        return false;
    }
}