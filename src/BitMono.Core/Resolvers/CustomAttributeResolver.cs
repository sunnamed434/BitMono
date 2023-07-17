namespace BitMono.Core.Resolvers;

[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
[SuppressMessage("ReSharper", "InvertIf")]
public static class CustomAttributeResolver
{
    [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
    public static List<CustomAttributeResolve> Resolve(IHasCustomAttribute from, string @namespace, string name)
    {
        var attributes = new List<CustomAttributeResolve>();
        var customAttributes = from.CustomAttributes;
        for (var i = 0; i < customAttributes.Count; i++)
        {
            var customAttribute = customAttributes[i];
            if (customAttribute.Constructor?.DeclaringType?.IsTypeOf(@namespace, name) == true)
            {
                if (customAttribute.Signature != null)
                {
                    var namedValues = new Dictionary<string, object>();
                    var fixedValues = new List<object>();
                    var namedArguments = customAttribute.Signature.NamedArguments;
                    for (var j = 0; j < namedArguments.Count; j++)
                    {
                        var namedArgument = namedArguments[j];
                        if (string.IsNullOrWhiteSpace(namedArgument.MemberName?.Value) == false)
                        {
                            if (namedArgument.Argument.Element is Utf8String utf8String)
                            {
                                namedValues.Add(namedArgument.MemberName!.Value, utf8String.Value);
                            }
                            else
                            {
                                if (namedArgument.Argument.Element != null)
                                {
                                    namedValues.Add(namedArgument.MemberName!.Value, namedArgument.Argument.Element);
                                }
                            }
                        }
                    }
                    var fixedArguments = customAttribute.Signature!.FixedArguments;
                    for (var k = 0; k < fixedArguments.Count; k++)
                    {
                        var fixedArgument = fixedArguments[k];
                        if (fixedArgument.Element is Utf8String utf8String)
                        {
                            fixedValues.Add(utf8String.Value);
                        }
                        else
                        {
                            if (fixedArgument.Element != null)
                            {
                                fixedValues.Add(fixedArgument.Element);
                            }
                        }
                    }

                    attributes.Add(new CustomAttributeResolve
                    {
                        NamedValues = namedValues,
                        FixedValues = fixedValues,
                        Attribute = customAttribute
                    });
                }
            }
        }
        return attributes;
    }
}