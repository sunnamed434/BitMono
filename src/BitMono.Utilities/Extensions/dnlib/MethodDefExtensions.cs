namespace BitMono.Utilities.Extensions.dnlib;

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
    public static MethodDef SetDeclaringTypeToNull(this MethodDef source)
    {
        source.DeclaringType = null;
        return source;
    }
}