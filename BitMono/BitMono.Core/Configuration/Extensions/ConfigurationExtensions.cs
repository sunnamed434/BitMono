using BitMono.API.Protecting;
using BitMono.Core.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace BitMono.Core.Configuration.Extensions
{
    public static class ConfigurationExtensions
    {
        public static ICollection<ProtectionSettings> GetProtectionSettings(this IConfiguration configuration)
        {
            return configuration.GetSection("Protections").Get<List<ProtectionSettings>>();
        }
        public static ICollection<string> GetCriticalMethods(this IConfiguration configuration)
        {
            return configuration.GetSection("CriticalMethods").Get<List<string>>();
        }
        public static ICollection<string> GetCriticalInterfaces(this IConfiguration configuration)
        {
            return configuration.GetSection("CriticalInterfaces").Get<string[]>();
        }
        public static ICollection<string> GetCriticalBaseTypes(this IConfiguration configuration)
        {
            return configuration.GetSection("CriticalBaseTypes").Get<string[]>();
        }
        public static bool AsProtection(this ProtectionSettings protectionSettings, ICollection<IProtection> protections, out IProtection result)
        {
            foreach (var protection in protections)
            {
                if (protection.GetType().Name.Equals(protectionSettings.Name))
                {
                    return (result = protection) != null;
                }
            }
            result = null;
            return false;
        }
    }
}