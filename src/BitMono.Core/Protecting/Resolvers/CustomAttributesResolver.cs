namespace BitMono.Core.Protecting.Resolvers;

public class CustomAttributesResolver : ICustomAttributesResolver
{
    [return: AllowNull]
    public IEnumerable<TAttribute> Resolve<TAttribute>(IHasCustomAttribute from, [AllowNull] Func<TAttribute, bool> strip)
        where TAttribute : Attribute
    {
        for (var i = 0; i < from.CustomAttributes.Count; i++)
        {
            TAttribute attribute = null;
            var customAttribute = from.CustomAttributes[i];
            foreach (var customAttributeProperty in customAttribute.Properties)
            {
                if (customAttribute.TypeFullName.Equals(typeof(TAttribute).FullName))
                {
                    attribute = Activator.CreateInstance<TAttribute>();
                    var propertyInfo = typeof(TAttribute).GetProperty(customAttributeProperty.Name);
                    if (customAttributeProperty.Value is UTF8String utf8String)
                    {
                        propertyInfo.SetValue(attribute, utf8String.String);
                    }
                    else
                    {
                        propertyInfo.SetValue(attribute, customAttributeProperty.Value);
                    }
                    if (strip?.Invoke(attribute) == true)
                    {
                        from.CustomAttributes.RemoveAt(i);
                    }
                }
            }
            yield return attribute;
        }
    }
}