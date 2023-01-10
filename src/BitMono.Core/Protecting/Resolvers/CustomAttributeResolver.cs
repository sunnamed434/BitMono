namespace BitMono.Core.Protecting.Resolvers;

public class CustomAttributeResolver
{
    [return: AllowNull]
    public CustomAttributeResolve Resolve(IHasCustomAttribute from, string @namespace, string name)
    {
        var keyValuePairs = new Dictionary<string, object>();
        for (var i = 0; i < from.CustomAttributes.Count; i++)
        {
            var customAttribute = from.CustomAttributes[i];
            if (customAttribute.Constructor.DeclaringType.IsTypeOf(@namespace, name))
            {
                foreach (var namedArgument in customAttribute.Signature.NamedArguments)
                {
                    if (namedArgument.Argument.Element is Utf8String utf8String)
                    {
                        keyValuePairs.Add(namedArgument.MemberName.Value, utf8String.Value);
                    }
                    else
                    {
                        keyValuePairs.Add(namedArgument.MemberName.Value, namedArgument.Argument.Element);
                    }
                }
                return new CustomAttributeResolve
                {
                    KeyValuePairs = keyValuePairs,
                    Attribute = customAttribute
                };
            }
        }
        return null;
    }
}