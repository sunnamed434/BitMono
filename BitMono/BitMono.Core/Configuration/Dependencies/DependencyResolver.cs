using BitMono.API.Protecting;
using BitMono.Core.Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace BitMono.Core.Configuration.Dependencies
{
    public class DependencyResolver
    {
        private readonly ICollection<IProtection> m_Protections;
        private readonly ICollection<ProtectionSettings> m_ProtectionSettings;
        private readonly ILogger m_Logger;

        public DependencyResolver(ICollection<IProtection> protections, ICollection<ProtectionSettings> protectionSettings, ILogger logger)
        {
            m_Protections = protections;
            m_ProtectionSettings = protectionSettings;
            m_Logger = logger;
        }


        public ICollection<IProtection> Sort(out ICollection<string> disabled)
        {
            List<IProtection> foundProtections = new List<IProtection>();
            var cachedProtections = m_Protections.ToArray().ToList();
            disabled = new List<string>();
            foreach (var protectionSettings in m_ProtectionSettings.Where(p => p.Enabled))
            {
                var protection = cachedProtections.FirstOrDefault(p => p.GetType().Name.Equals(protectionSettings.Name));
                if (protection != null)
                {
                    foundProtections.Add(protection);
                    cachedProtections.Remove(protection);
                }
                else
                {
                    m_Logger.Warning("Protection: {0}, does not exsist in current context!", protectionSettings.Name);
                }
            }
            foreach (var protection in cachedProtections)
            {
                disabled.Add(protection.GetType().Name);
            }
            return foundProtections;
        }
    }
}