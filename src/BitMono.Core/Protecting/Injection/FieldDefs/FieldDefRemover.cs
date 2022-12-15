namespace BitMono.Core.Protecting.Injection.FieldDefs;

public class FieldDefRemover : IFieldRemover
{
    public bool Remove(string name, ModuleDefMD module)
    {
        foreach (TypeDef typeDef in module.Types)
        {
            foreach (var fieldDef in typeDef.Fields)
            {
                if (fieldDef.Name.Equals(name))
                {
                    typeDef.Fields.Remove(fieldDef);
                    return true;
                }
            }
        }
        return false;
    }
}