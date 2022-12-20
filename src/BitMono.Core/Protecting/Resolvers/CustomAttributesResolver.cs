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
            foreach (var customAttributeNamedArgument in customAttribute.Signature.NamedArguments)
            {
                if (customAttribute.Constructor.DeclaringType.Equals(typeof(TAttribute).FullName))
                {
                    attribute = Activator.CreateInstance<TAttribute>();
                    var propertyInfo = typeof(TAttribute).GetProperty(customAttributeNamedArgument.MemberName);
                    propertyInfo.SetValue(attribute, customAttributeNamedArgument.Argument.Element);
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