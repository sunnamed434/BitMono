namespace BitMono.Utilities.Extensions.AsmResolver;

public static class MethodDefinitionExtensions
{
    public static bool IsAsync(this MethodDefinition source)
    {
        foreach (var typeDef in source.Module.GetAllTypes())
        {
            foreach (var nestedTypeDef in typeDef.NestedTypes.Where(n => n.Name.StartsWith("<")))
            {
                if (nestedTypeDef.IsValueType
                    && nestedTypeDef.Interfaces.Any()
                    && nestedTypeDef.Attributes == TypeAttributes.AutoLayout
                    && nestedTypeDef.Interfaces.Any(i => i.Interface.Name.Equals(nameof(IAsyncStateMachine)))
                    && nestedTypeDef.Name.Contains(source.Name))
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
}