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
    public static RuntimeMonikerAttribute[] GetRuntimeMonikerAttributes(this Type source, bool inherit = false)
    {
        return source
            .GetCustomAttributes<RuntimeMonikerAttribute>(inherit)
            .ToArray();
    }
    public static RuntimeMonikerAttribute[] GetRuntimeMonikerAttributes(this IProtection source)
    {
        return source
            .GetType()
            .GetRuntimeMonikerAttributes();
    }
    public static ConfigureForNativeCodeAttribute? GetConfigureForNativeCodeAttribute(this Type source, bool inherit = false)
    {
        return source.GetCustomAttribute<ConfigureForNativeCodeAttribute>(inherit);
    }
    public static ConfigureForNativeCodeAttribute? GetConfigureForNativeCodeAttribute(this IProtection source)
    {
        return source
            .GetType()
            .GetConfigureForNativeCodeAttribute();
    }
    public static IL2CPPIncompatibleAttribute? GetIL2CPPIncompatibleAttribute(this Type source, bool inherit = false)
    {
        return source.GetCustomAttribute<IL2CPPIncompatibleAttribute>(inherit);
    }
    public static IL2CPPIncompatibleAttribute? GetIL2CPPIncompatibleAttribute(this IProtection source)
    {
        return source
            .GetType()
            .GetIL2CPPIncompatibleAttribute();
    }
    /// <summary>
    /// A protection is IL2CPP-incompatible when it is explicitly marked with
    /// <see cref="IL2CPPIncompatibleAttribute"/>, or when it emits native code
    /// (<see cref="ConfigureForNativeCodeAttribute"/>) - native method bodies can never be converted
    /// to C++ by il2cpp.exe. See #250.
    /// </summary>
    public static bool IsIL2CPPIncompatible(this Type source)
    {
        return source.GetIL2CPPIncompatibleAttribute() != null
            || source.GetConfigureForNativeCodeAttribute() != null;
    }
    public static bool IsIL2CPPIncompatible(this IProtection source)
    {
        return source.GetType().IsIL2CPPIncompatible();
    }
    /// <summary>
    /// The user-facing reason a protection is skipped on IL2CPP builds.
    /// </summary>
    public static string GetIL2CPPIncompatibleReason(this IProtection source)
    {
        var attribute = source.GetIL2CPPIncompatibleAttribute();
        if (attribute != null)
        {
            return attribute.GetMessage();
        }
        if (source.GetConfigureForNativeCodeAttribute() != null)
        {
            return "Emits native (unmanaged) method bodies, which IL2CPP cannot convert to C++";
        }
        return "Not supported on IL2CPP builds";
    }
    public static bool TryGetObsoleteAttribute(this Type source, out ObsoleteAttribute? attribute, bool inherit = false)
    {
        attribute = source.GetCustomAttribute<ObsoleteAttribute>(inherit);
        if (attribute == null)
        {
            return false;
        }
        return true;
    }
    public static bool TryGetObsoleteAttribute(this IProtection source, out ObsoleteAttribute? attribute)
    {
        return source.GetType().TryGetObsoleteAttribute(out attribute);
    }
    public static string GetName(this Type source, bool inherit = false)
    {
        var protectionNameAttribute = source.GetCustomAttribute<ProtectionNameAttribute>(inherit);
        if (protectionNameAttribute != null)
        {
            return !string.IsNullOrWhiteSpace(protectionNameAttribute.Name)
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