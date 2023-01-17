namespace BitMono.Core.Extensions;

public static class ConfigurationExtensions
{
    public static List<ProtectionSetting> GetProtectionSettings(this IBitMonoProtectionsConfiguration source)
    {
        return source.Configuration.GetProtectionSettings();
    }
    public static List<CriticalAttribute> GetCriticalModelAttributes(this IConfiguration source)
    {
        return source.GetSection(nameof(Criticals.CriticalModelAttributes)).Get<List<CriticalAttribute>>();
    }
    public static List<CriticalAttribute> GetCriticalModelAttributes(this IBitMonoProtectionsConfiguration source)
    {
        return GetCriticalModelAttributes(source.Configuration);
    }
    public static List<ProtectionSetting> GetProtectionSettings(this IConfiguration source)
    {
        return source.GetSection("Protections").Get<List<ProtectionSetting>>();
    }
    public static List<CriticalAttribute> GetCriticalAttributes(this IBitMonoCriticalsConfiguration source)
    {
        return GetCriticalAttributes(source.Configuration);
    }
    public static List<CriticalAttribute> GetCriticalAttributes(this IConfiguration source)
    {
        return source.GetSection(nameof(Criticals.CriticalAttributes)).Get<List<CriticalAttribute>>();
    }
    public static List<string> GetCriticalMethods(this IBitMonoCriticalsConfiguration source)
    {
        return GetCriticalMethods(source.Configuration);
    }
    public static List<string> GetCriticalMethods(this IConfiguration source)
    {
        return source.GetSection(nameof(Criticals.CriticalMethods)).Get<List<string>>();
    }
    public static string[] GetCriticalInterfaces(this IBitMonoCriticalsConfiguration source)
    {
        return source.Configuration.GetSection(nameof(Criticals.CriticalInterfaces)).Get<string[]>();
    }
    public static string[] GetCriticalInterfaces(this IConfiguration source)
    {
        return source.GetSection(nameof(Criticals.CriticalInterfaces)).Get<string[]>();
    }
    public static string[] GetCriticalBaseTypes(this IBitMonoCriticalsConfiguration source)
    {
        return source.Configuration.GetSection(nameof(Criticals.CriticalBaseTypes)).Get<string[]>();
    }
    public static string[] GetCriticalBaseTypes(this IConfiguration source)
    {
        return source.GetSection(nameof(Criticals.CriticalBaseTypes)).Get<string[]>();
    }
    public static string[] GetSpecificNamespaces(this IBitMonoObfuscationConfiguration source)
    {
        return GetSpecificNamespaces(source.Configuration);
    }
    public static string[] GetSpecificNamespaces(this IConfiguration source)
    {
        return source.GetSection(nameof(Obfuscation.SpecificNamespaces)).Get<string[]>();
    }
    public static string[] GetRandomStrings(this IBitMonoObfuscationConfiguration source)
    {
        return GetRandomStrings(source.Configuration);
    }
    public static string[] GetRandomStrings(this IConfiguration source)
    {
        return source.GetSection(nameof(Obfuscation.RandomStrings)).Get<string[]>();
    }
    public static bool AsProtection(this ProtectionSetting source, ICollection<IProtection> protections, out IProtection result)
    {
        foreach (var protection in protections)
        {
            if (protection.GetType().Name.Equals(source.Name))
            {
                return (result = protection) != null;
            }
        }
        result = null;
        return false;
    }
}