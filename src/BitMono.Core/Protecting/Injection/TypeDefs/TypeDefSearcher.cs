namespace BitMono.Core.Protecting.Injection.TypeDefs;

public class TypeDefSearcher : ITypeDefSearcher
{
    [return: AllowNull]
    public TypeDef Find(string name, ModuleDefMD moduleDefMD)
    {
        foreach (var typeDef in moduleDefMD.GetTypes().ToArray())
        {
            if (typeDef.Name.Equals(name))
            {
                return typeDef;
            }
        }
        return null;
    }
    [return: AllowNull]
    public TypeDef FindInGlobalNestedTypes(string name, ModuleDefMD moduleDefMD)
    {
        foreach (var typeDef in moduleDefMD.GlobalType.NestedTypes.ToArray())
        {
            if (typeDef.Name.Equals(name))
            {
                return typeDef;
            }
        }
        return null;
    }
}