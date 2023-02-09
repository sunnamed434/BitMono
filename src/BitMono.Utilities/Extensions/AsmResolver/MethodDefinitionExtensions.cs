namespace BitMono.Utilities.Extensions.AsmResolver;

public static class MethodDefinitionExtensions
{
    public static bool IsAsync(this MethodDefinition source)
    {
        foreach (var type in source.Module.GetAllTypes())
        {
            foreach (var nestedType in type.NestedTypes.Where(n => n.Name.StartsWith("<")))
            {
                if (nestedType.IsValueType
                    && nestedType.Attributes == TypeAttributes.AutoLayout
                    && nestedType.Interfaces.Any(i => i.Interface.Name.Equals(nameof(IAsyncStateMachine)))
                    && nestedType.Name.Contains(source.Name))
                {
                    return true;
                }
            }
        }
        return false;
    }
    public static bool NotAsync(this MethodDefinition source)
    {
        return source.IsAsync() == false;
    }
    public static bool IsGetterAndSetter(this MethodDefinition source)
    {
        return source.IsGetMethod == true && source.IsSetMethod == true;
    }
    public static bool NotGetterAndSetter(this MethodDefinition source)
    {
        return source.IsGetMethod == false && source.IsSetMethod == false;
    }
    public static bool HasParameters(this MethodDefinition source)
    {
        return source.Parameters.Any();
    }
    public static bool ReturnsValue(this MethodSignature source, TypeSignature typeSignature)
    {
        return source.ReturnsValue && source.ReturnType.Equals(typeSignature);
    }
}