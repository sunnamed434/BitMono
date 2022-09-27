using dnlib.DotNet;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TypeAttributes = dnlib.DotNet.TypeAttributes;

namespace BitMono.Utilities.Extensions.Dnlib
{
    public static class MethodDefExtensions
    {
        public static bool IsAsync(this MethodDef source)
        {
            foreach (var typeDef in source.Module.Types)
            {
                if (typeDef.HasNestedTypes)
                {
                    foreach (var nestedTypeDef in typeDef.NestedTypes.Where(n => n.Name.StartsWith("<")))
                    {
                        if (nestedTypeDef.IsValueType
                            && nestedTypeDef.HasInterfaces
                            && nestedTypeDef.Layout == TypeAttributes.AutoLayout
                            && nestedTypeDef.Interfaces.Any(i => i.Interface.Name.Equals(nameof(IAsyncStateMachine)))
                            && nestedTypeDef.Name.Contains(source.Name))
                        {
                            return true;
                        }
                    }

                }
            }
            return false;
        }
        public static bool NotAsync(this MethodDef source)
        {
            return source.IsAsync() == false;
        }
        public static bool IsGetterAndSetter(this MethodDef source)
        {
            return source.IsGetter == true && source.IsSetter == true;
        }
        public static bool NotGetterAndSetter(this MethodDef source)
        {
            return source.IsGetter == false && source.IsSetter == false;
        }
        public static bool HasParameters(this MethodDef source)
        {
            return source.Parameters.Any();
        }
        public static bool DeclaredInSameAssemblyAs(this MethodDef source, Assembly target)
        {
            return source.ToString().Contains(target.FullName.Split('.', ',')[0]);
        }
    }
}