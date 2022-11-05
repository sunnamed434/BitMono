using BitMono.API.Protecting.Resolvers;
using dnlib.DotNet;
using NullGuard;
using System;
using System.Collections.Generic;

namespace BitMono.Core.Protecting.Resolvers
{
    public class CustomAttributesResolver : ICustomAttributesResolver
    {
        [return: AllowNull]
        public IEnumerable<TAttribute> Resolve<TAttribute>(IHasCustomAttribute from)
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
                        yield return attribute;
                    }
                }
            }
        }
    }
}