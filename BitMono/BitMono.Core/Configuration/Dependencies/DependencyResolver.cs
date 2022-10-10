using BitMono.API.Protecting;
using BitMono.Core.Models;
using BitMono.Core.Protecting;
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
            List<IProtection> protections = new List<IProtection>();
            disabled = new List<string>();
            foreach (var protectionSettings in m_ProtectionSettings.Where(p => p.Enabled))
            {
                var protection = m_Protections.FirstOrDefault(p => p.GetType().Name.Equals(protectionSettings.Name));
                if (protection != null)
                {
                    protections.Add(protection);
                    m_Protections.Remove(protection);
                }
                else
                {
                    m_Logger.Warning($"Protection: {protectionSettings.Name}, does not exsist in current context!");
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