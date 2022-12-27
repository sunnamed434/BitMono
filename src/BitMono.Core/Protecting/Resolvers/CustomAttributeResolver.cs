namespace BitMono.Core.Protecting.Resolvers;

public class CustomAttributeResolver : ICustomAttributeResolver
{
    [return: AllowNull]
    public Dictionary<string, CustomAttributesResolve> Resolve(IHasCustomAttribute from, Type attributeType)
    {
        var keyValuePairs = new Dictionary<string, CustomAttributesResolve>();
        for (var i = 0; i < from.CustomAttributes.Count; i++)
        {
            var customAttribute = from.CustomAttributes[i];
            foreach (var customAttributeNamedArgument in customAttribute.Signature.NamedArguments)
            {
                if (customAttribute.Constructor.DeclaringType.FullName.Equals(attributeType.FullName))
                {
                    if (customAttributeNamedArgument.Argument.Element is Utf8String utf8String)
                    {
                        keyValuePairs.Add(customAttributeNamedArgument.MemberName.Value, new CustomAttributesResolve
                        {
                            Value = utf8String.Value,
                            CustomAttribute = customAttribute
                        });
                    }
                    else
                    {
                        keyValuePairs.Add(customAttributeNamedArgument.MemberName.Value, new CustomAttributesResolve
                        {
                            Value = customAttributeNamedArgument.Argument.Element,
                            CustomAttribute = customAttribute
                        });
                    }
                }
            }
        }
        Console.WriteLine(string.Join("\n", keyValuePairs.Select(k => $"Property: {k.Key}, Type: {k.Value.GetType()}")));
        return keyValuePairs;
    }
}
/*
public class CustomAttributesResolver : ICustomAttributesResolver
{
    [return: AllowNull]
    public IEnumerable<TAttribute> Resolve<TAttribute>(IHasCustomAttribute from)
        where TAttribute : Attribute
    {
        for (var i = 0; i < from.CustomAttributes.Count; i++)
        {
            TAttribute attribute = null;
            var customAttribute = from.CustomAttributes[i];
            foreach (var customAttributeNamedArgument in customAttribute.Signature.NamedArguments)
            {
                if (customAttribute.Constructor.DeclaringType.FullName.Equals(typeof(TAttribute).FullName))
                {
                    attribute = Activator.CreateInstance<TAttribute>();
                    var propertyInfo = typeof(TAttribute).GetProperty(customAttributeNamedArgument.MemberName);
                    if (customAttributeNamedArgument.Argument.Element is Utf8String utf8String)
                    {
                        propertyInfo.SetValue(attribute, utf8String.Value);
                    }
                    else
                    {
                        propertyInfo.SetValue(attribute, customAttributeNamedArgument.Argument.Element);
                    }
                }
            }
            yield return attribute;
        }
    }
}*/