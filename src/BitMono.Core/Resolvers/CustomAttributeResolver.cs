namespace BitMono.Core.Resolvers;

[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
[SuppressMessage("ReSharper", "InvertIf")]
public static class CustomAttributeResolver
{
    [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
    public static List<CustomAttributeResolve> Resolve(IHasCustomAttribute from, string @namespace, string name)
    {
        var attributes = new List<CustomAttributeResolve>();
        for (var i = 0; i < from.CustomAttributes.Count; i++)
        {
            var customAttribute = from.CustomAttributes[i];
            if (customAttribute.Constructor?.DeclaringType?.IsTypeOf(@namespace, name) == true)
            {
                if (customAttribute.Signature != null)
                {
                    var namedValues = new Dictionary<string, object>();
                    var fixedValues = new List<object>();
                    foreach (var namedArgument in customAttribute.Signature.NamedArguments)
                    {
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
                    foreach (var fixedArgument in customAttribute.Signature!.FixedArguments)
                    {
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