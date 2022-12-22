namespace BitMono.Core.Extensions.Protections;

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
    public static string GetName(this IProtection source)
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