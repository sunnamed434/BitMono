namespace BitMono.Utilities.AsmResolver;

public static class MethodDefinitionExtensions
{
    public static bool IsAsync(this MethodDefinition source)
    {
        foreach (var type in source.Module.GetAllTypes())
        {
            foreach (var nestedType in type.NestedTypes.Where(x => x.Name.StartsWith("<")))
            {
                if (nestedType.IsValueType
                    && nestedType.Attributes == TypeAttributes.AutoLayout
                    && nestedType.Interfaces.Any(x => x.Interface.Name.Equals(nameof(IAsyncStateMachine)))
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
    public static bool HasParameters(this MethodDefinition source)
    {
        return source.Parameters.Any();
    }
    public static bool Returns(this MethodSignature source, TypeSignature typeSignature)
    {
        return source.ReturnType.Equals(typeSignature);
    }
}