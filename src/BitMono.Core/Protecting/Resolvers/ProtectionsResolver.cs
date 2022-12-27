namespace BitMono.Core.Protecting.Resolvers;

public class ProtectionsResolver
{
    private readonly List<IProtection> m_Protections;
    private readonly IEnumerable<ProtectionSettings> m_ProtectionSettings;
    private readonly ILogger m_Logger;

    public ProtectionsResolver(List<IProtection> protections, IEnumerable<ProtectionSettings> protectionSettings, ILogger logger)
    {
        m_Protections = protections;
        m_ProtectionSettings = protectionSettings;
        m_Logger = logger.ForContext<ProtectionsResolver>();
    }

    public ProtectionsResolve Sort()
    {
        var foundProtections = new List<IProtection>();
        var cachedProtections = m_Protections.ToArray().ToList();
        var disabledProtections = new List<string>();
        foreach (var protectionSettings in m_ProtectionSettings.Where(p => p.Enabled))
        {
            var protection = cachedProtections.FirstOrDefault(p => p.GetName().Equals(protectionSettings.Name, StringComparison.OrdinalIgnoreCase));
            if (protection != null)
            {
                foundProtections.Add(protection);
                cachedProtections.Remove(protection);
            }
            else
            {
                m_Logger.Warning("Protection: {0}, does not exist in current context!", protectionSettings.Name);
            }
        }
        foreach (var protection in cachedProtections)
        {
            disabledProtections.Add(protection.GetName());
        }
        return new ProtectionsResolve
        {
            FoundProtections = foundProtections,
            DisabledProtections = disabledProtections,
        };
    }
}