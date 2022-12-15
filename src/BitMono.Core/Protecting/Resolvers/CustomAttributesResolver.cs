namespace BitMono.Core.Protecting.Resolvers;

public class CustomAttributesResolver : ICustomAttributesResolver
{
    [return: AllowNull]
    public IEnumerable<TAttribute> Resolve<TAttribute>(IHasCustomAttribute from, [AllowNull] Func<TAttribute, bool> strip)
        where TAttribute : Attribute
    {
        for (int i = 0; i < from.CustomAttributes.Count; i++)
        {
            var attribute = Activator.CreateInstance<TAttribute>();
            var customAttribute = from.CustomAttributes[i];
            foreach (var customAttributeProperty in customAttribute.Properties)
            {
                if (customAttribute.TypeFullName.Equals(typeof(TAttribute).FullName))
                {
                    var propertyInfo = typeof(TAttribute).GetProperty(customAttributeProperty.Name);
                    if (customAttributeProperty.Value is UTF8String utf8String)
                    {
                        propertyInfo.SetValue(attribute, utf8String.String);
                    }
                    else
                    {
                        propertyInfo.SetValue(attribute, customAttributeProperty.Value);
                    }
                }
            }

            if (strip?.Invoke(attribute) == true)
            {
                from.CustomAttributes.RemoveAt(i);
            }
            yield return attribute;
        }
    }
}