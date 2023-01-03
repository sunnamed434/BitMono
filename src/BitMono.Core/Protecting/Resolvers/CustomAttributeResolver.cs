namespace BitMono.Core.Protecting.Resolvers;

public class CustomAttributeResolver : ICustomAttributeResolver
{
    [return: AllowNull]
    public Dictionary<string, CustomAttributeResolve> Resolve(IHasCustomAttribute from, string @namespace, string name)
    {
        var keyValuePairs = new Dictionary<string, CustomAttributeResolve>();
        for (var i = 0; i < from.CustomAttributes.Count; i++)
        {
            var customAttribute = from.CustomAttributes[i];
            foreach (var namedArgument in customAttribute.Signature.NamedArguments)
            {
                if (customAttribute.Constructor.DeclaringType.IsTypeOf(@namespace, name))
                {
                    if (namedArgument.Argument.Element is Utf8String utf8String)
                    {
                        keyValuePairs.Add(namedArgument.MemberName.Value, new CustomAttributeResolve
                        {
                            Value = utf8String.Value,
                            CustomAttribute = customAttribute
                        });
                    }
                    else
                    {
                        keyValuePairs.Add(namedArgument.MemberName.Value, new CustomAttributeResolve
                        {
                            Value = namedArgument.Argument.Element,
                            CustomAttribute = customAttribute
                        });
                    }
                }
            }
        }
        return keyValuePairs;
    }
}