namespace BitMono.Core.Protecting.Resolvers;

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
}