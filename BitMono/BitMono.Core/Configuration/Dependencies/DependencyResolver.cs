using BitMono.API.Protecting;
using BitMono.Core.Logging;
using BitMono.Core.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace BitMono.Core.Configuration.Dependencies
{
    public class DependencyResolver
    {
        private readonly ICollection<IProtection> m_Protections;
        private readonly IConfiguration m_Configuration;
        private readonly ILogger m_Logger;

        public DependencyResolver(ICollection<IProtection> protections, IConfiguration configuration, ILogger logger)
        {
            m_Protections = protections;
            m_Configuration = configuration;
            m_Logger = logger;
        }


        public ICollection<IProtection> Sort(out ICollection<string> disabled)
        {
            List<IProtection> protections = new List<IProtection>();
            disabled = new List<string>();
            var protectionsSettings = m_Configuration.GetSection("Protections").Get<List<ProtectionSettings>>();
            foreach (var item in protectionsSettings.Where(p => p.Enabled))
            {
                var protection = m_Protections.FirstOrDefault(p => p.GetType().Name.Equals(item.Name));
                if (protection != null)
                {
                    protections.Add(protection);
                    m_Protections.Remove(protection);
                }
                else
                {
                    m_Logger.Warn($"Protection: {item.Name}, not exsist in current context!");
                }
            }
            foreach (var protection in m_Protections)
            {
                disabled.Add(protection.GetType().Name);
            }
            return protections;
        }
    }
}