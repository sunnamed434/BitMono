using BitMono.API.Resolvers;

namespace BitMono.Core.Resolvers;

public class CustomAttributeResolver
{
    public IEnumerable<CustomAttributeResolve>? Resolve(IHasCustomAttribute from, string @namespace, string name)
    {
        for (var i = 0; i < from.CustomAttributes.Count; i++)
        {
            var customAttribute = from.CustomAttributes[i];
            if (customAttribute.Constructor.DeclaringType.IsTypeOf(@namespace, name))
            {
                var namedValues = new Dictionary<string, object>();
                var fixedValues = new List<object>();
                foreach (var namedArgument in customAttribute.Signature.NamedArguments)
                {
                    if (namedArgument.Argument.Element is Utf8String utf8String)
                    {
                        namedValues.Add(namedArgument.MemberName.Value, utf8String.Value);
                    }
                    else
                    {
                        namedValues.Add(namedArgument.MemberName.Value, namedArgument.Argument.Element);
                    }
                }
                foreach (var fixedArgument in customAttribute.Signature.FixedArguments)
                {
                    if (fixedArgument.Element is Utf8String utf8String)
                    {
                        fixedValues.Add(utf8String.Value);
                    }
                    else
                    {
                        fixedValues.Add(fixedArgument.Element);
                    }
                }
                yield return new CustomAttributeResolve
                {
                    NamedValues = namedValues,
                    FixedValues = fixedValues,
                    Attribute = customAttribute
                };
            }
        }
    }
}