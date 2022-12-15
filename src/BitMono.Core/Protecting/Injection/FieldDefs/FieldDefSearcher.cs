namespace BitMono.Core.Protecting.Injection.FieldDefs;

public class FieldDefSearcher : IFieldSearcher
{
    [return: AllowNull]
    public FieldDef Find(string name, ModuleDefMD moduleDefMD)
    {
        foreach (var typeDef in moduleDefMD.Types)
        {
            foreach (var fieldDef in typeDef.Fields)
            {
                if (fieldDef.Name.Equals(name))
                {
                    return fieldDef;
                }
            }
        }
        return null;
    }
    [return: AllowNull]
    public FieldDef FindInGlobalNestedTypes(string name, ModuleDefMD moduleDefMD)
    {
        foreach (var type in moduleDefMD.GlobalType.NestedTypes)
        {
            if (type.HasFields)
            {
                foreach (var childField in type.Fields)
                {
                    if (childField.Name.Equals(name))
                    {
                        return childField;
                    }
                }
            }
        }
        return null;
    }
}