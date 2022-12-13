using BitMono.API.Protecting;
using BitMono.Core.Protecting.Attributes;
using System;
using System.Reflection;

namespace BitMono.Utilities.Extensions
{
    public static class ProtectionExtensions
    {
        public static string GetName(this Type source, bool inherit = false)
        {
            var protectionNameAttribute = source.GetCustomAttribute<ProtectionNameAttribute>(inherit);
            if (protectionNameAttribute != null)
            {
                if (string.IsNullOrWhiteSpace(protectionNameAttribute.Name) == false)
                {
                    return protectionNameAttribute.Name;
                }
                else
                {
                    return source.Name;
                }
            }
            else
            {
                return source.Name;
            }
        }
        public static string GetName(this IProtection source, bool inherit = false)
        {
            return GetName(source.GetType(), inherit);
        }
        public static string GetName(this IPacker source,  bool inherit = false)
        {
            return GetName(source.GetType(), inherit);
        }
        public static string GetName<TProtection>(bool inherit = false) where TProtection : IProtection
        {
            return GetName(typeof(TProtection), inherit);
        }
    }
}