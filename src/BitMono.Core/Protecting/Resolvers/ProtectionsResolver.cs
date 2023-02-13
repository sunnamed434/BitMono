namespace BitMono.Core.Protecting.Resolvers;

public class ProtectionsResolver
{
    private readonly List<IProtection> m_Protections;
    private readonly IEnumerable<ProtectionSetting> m_ProtectionSettings;

    public ProtectionsResolver(List<IProtection> protections, IEnumerable<ProtectionSetting> protectionSettings)
    {
        m_Protections = protections;
        m_ProtectionSettings = protectionSettings;
    }

    public ProtectionsResolve Sort()
    {
        var foundProtections = new List<IProtection>();
        var cachedProtections = m_Protections.ToArray().ToList();
        var unknownProtections = new List<string>();
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
                unknownProtections.Add(protectionSettings.Name);
            }
        }
        var disabledProtections = cachedProtections.Select(protection => protection.GetName()).ToList();
        return new ProtectionsResolve
        {
            FoundProtections = foundProtections,
            DisabledProtections = disabledProtections,
            UnknownProtections = unknownProtections
        };
    }
}