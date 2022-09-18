using BitMono.API.Injection.Fields;
using dnlib.DotNet;

namespace BitMono.Core.Injection.Fields
{
    public class FieldRemover : IFieldRemover
    {
        public bool Remove(string name, ModuleDefMD module)
        {
            foreach (TypeDef type in module.Types)
            {
                foreach (var field in type.Fields)
                {
                    if (field.Name.Equals(name))
                    {
                        type.Fields.Remove(field);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}