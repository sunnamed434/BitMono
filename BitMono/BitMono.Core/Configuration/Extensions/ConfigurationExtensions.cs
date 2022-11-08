using BitMono.API.Configuration;
using BitMono.API.Protecting;
using BitMono.Core.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace BitMono.Core.Configuration.Extensions
{
    public static class ConfigurationExtensions
    {
        public static List<ProtectionSettings> GetProtectionSettings(this IBitMonoProtectionsConfiguration source)
        {
            return source.Configuration.GetProtectionSettings();
        }
        public static List<ProtectionSettings> GetProtectionSettings(this IConfiguration source)
        {
            return source.GetSection("Protections").Get<List<ProtectionSettings>>();
        }
        public static List<string> GetCriticalMethods(this IBitMonoCriticalsConfiguration source)
        {
            return source.GetCriticalMethods();
        }
        public static List<string> GetCriticalMethods(this IConfiguration source)
        {
            return source.GetSection("CriticalMethods").Get<List<string>>();
        }
        public static string[] GetCriticalInterfaces(this IBitMonoCriticalsConfiguration source)
        {
            return source.Configuration.GetSection("CriticalInterfaces").Get<string[]>();
        }
        public static string[] GetCriticalInterfaces(this IConfiguration source)
        {
            return source.GetSection("CriticalInterfaces").Get<string[]>();
        }
        public static string[] GetCriticalBaseTypes(this IBitMonoCriticalsConfiguration source)
        {
            return source.Configuration.GetSection("CriticalBaseTypes").Get<string[]>();
        }
        public static string[] GetCriticalBaseTypes(this IConfiguration source)
        {
            return source.GetSection("CriticalBaseTypes").Get<string[]>();
        }
        public static string[] GetStrings(this IBitMonoObfuscationConfiguration source)
        {
            return source.Configuration.GetSection("Strings").Get<string[]>();
        }
        public static string[] GetStrings(this IConfiguration source)
        {
            return source.GetSection("Strings").Get<string[]>();
        }
        public static bool AsProtection(this ProtectionSettings source, ICollection<IProtection> protections, out IProtection result)
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
}