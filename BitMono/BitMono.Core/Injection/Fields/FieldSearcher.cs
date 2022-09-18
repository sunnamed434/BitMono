using BitMono.API.Injection.Fields;
using dnlib.DotNet;

namespace BitMono.Core.Injection.Fields
{
    public class FieldSearcher : IFieldSearcher
    {
        public FieldDef Find(string name, ModuleDefMD moduleDefMD)
        {
            foreach (var type in moduleDefMD.Types)
            {
                foreach (var field in type.Fields)
                {
                    if (field.Name.Equals(name))
                    {
                        return field;
                    }
                }
            }
            return null;
        }
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
}