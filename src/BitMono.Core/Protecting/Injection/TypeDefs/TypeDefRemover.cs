namespace BitMono.Core.Protecting.Injection.TypeDefs;

public class TypeDefRemover : ITypeDefRemover
{
    public bool Remove(string name, ModuleDefMD module)
    {
        foreach (var typeDef in module.GetTypes().ToArray())
        {
            if (typeDef.Name.Equals(name))
            {
                if (typeDef.IsNested)
                {
                    typeDef.NestedTypes.Remove(typeDef);
                }
                else
                {
                    module.Types.Remove(typeDef);
                }
                return true;
            }
        }
        return false;
    }
}