namespace BitMono.Core.Resolvers;

public class ProtectionsResolver
{
    private readonly List<IProtection> _protections;
    private readonly IEnumerable<ProtectionSetting> _protectionSettings;

    public ProtectionsResolver(List<IProtection> protections, IEnumerable<ProtectionSetting> protectionSettings)
    {
        _protections = protections;
        _protectionSettings = protectionSettings;
    }

    public ProtectionsResolve Sort()
    {
        var foundProtections = new List<IProtection>();
        var cachedProtections = _protections.ToArray().ToList();
        var unknownProtections = new List<string>();
        foreach (var protectionSettings in _protectionSettings.Where(x => x.Enabled))
        {
            var protection = cachedProtections.FirstOrDefault(x =>
                x.GetName().Equals(protectionSettings.Name, StringComparison.OrdinalIgnoreCase));
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
        var disabledProtections = cachedProtections
            .Select(x => x.GetName())
            .ToList();
        return new ProtectionsResolve
        {
            FoundProtections = foundProtections,
            DisabledProtections = disabledProtections,
            UnknownProtections = unknownProtections
        };
    }
}