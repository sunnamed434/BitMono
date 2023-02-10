namespace BitMono.Core.Extensions;

public static class ProtectionExtensions
{
    public static bool TryGetDoNotResolveAttribute(this Type source, out DoNotResolveAttribute? attribute, bool inherit = false)
    {
        attribute = source.GetCustomAttribute<DoNotResolveAttribute>(inherit);
        if (attribute == null)
        {
            return false;
        }
        return true;
    }
    public static bool TryGetDoNotResolveAttribute(this IProtection source, out DoNotResolveAttribute? attribute)
    {
        return source.GetType().TryGetDoNotResolveAttribute(out attribute);
    }
    public static bool TryGetDoNotResolveAttribute(this IPacker source, out DoNotResolveAttribute? attribute)
    {
        return source.GetType().TryGetDoNotResolveAttribute(out attribute);
    }
    public static bool TryGetDoNotResolveAttribute<TProtection>(out DoNotResolveAttribute? attribute) where TProtection : IProtection
    {
        return typeof(TProtection).TryGetDoNotResolveAttribute(out attribute);
    }
    public static bool TryGetDependOnRuntimeAttribute(this Type source, out DependOnRuntimeAttribute? attribute, bool inherit = false)
    {
        attribute = source.GetCustomAttribute<DependOnRuntimeAttribute>(inherit);
        if (attribute == null)
        {
            return false;
        }
        return true;
    }
    public static bool TryGetDependOnRuntimeAttribute(this IProtection source, out DependOnRuntimeAttribute? attribute)
    {
        return source.GetType().TryGetDependOnRuntimeAttribute(out attribute);
    }
    public static string GetName(this Type source, bool inherit = false)
    {
        var protectionNameAttribute = source.GetCustomAttribute<ProtectionNameAttribute>(inherit);
        if (protectionNameAttribute != null)
        {
            return string.IsNullOrWhiteSpace(protectionNameAttribute.Name) == false
                ? protectionNameAttribute.Name
                : source.Name;
        }
        return source.Name;
    }
    public static string GetName(this IProtection source)
    {
        return source.GetType().GetName(inherit: false);
    }
    public static string GetName(this IPipelineProtection source)
    {
        return source.GetType().GetName(inherit: false);
    }
    public static string GetName(this IPacker source)
    {
        return source.GetType().GetName(inherit: false);
    }
    public static string GetName<TProtection>() where TProtection : IProtection
    {
        return typeof(TProtection).GetName(inherit: false);
    }
}