namespace BitMono.Obfuscation;

public class ProtectionsNotifier
{
    private readonly IConfiguration m_Configuration;
    private readonly ILogger m_Logger;

    public ProtectionsNotifier(IBitMonoObfuscationConfiguration configuration, ILogger logger)
    {
        m_Configuration = configuration.Configuration;
        m_Logger = logger.ForContext<ProtectionsNotifier>();
    }

    public void Notify(ProtectionsSort protectionsSort)
    {
        if (m_Configuration.GetValue<bool>(nameof(Shared.Models.Obfuscation.NotifyProtections)))
        {
            if (protectionsSort.HasProtections)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(string.Join(", ", protectionsSort.SortedProtections.Select(p => p.GetName())));
                if (protectionsSort.Pipelines.Any())
                {
                    stringBuilder.Append(", ");
                    stringBuilder.Append(string.Join(", ", protectionsSort.Pipelines.Select(p => p.GetName())));
                }
                if (protectionsSort.Packers.Any())
                {
                    stringBuilder.Append(", ");
                    stringBuilder.Append(string.Join(", ", protectionsSort.Packers.Select(p => p.GetName())));
                }
                m_Logger.Information("Enabled protections: {0}", stringBuilder.ToString());
            }
            if (protectionsSort.DeprecatedProtections.Any())
            {
                m_Logger.Warning("Skip deprecated protections: {0}", string.Join(", ", protectionsSort.DeprecatedProtections.Select(p => p?.GetName())));
            }
            if (protectionsSort.ProtectionsResolve.DisabledProtections.Any())
            {
                m_Logger.Warning("Disabled protections: {0}", string.Join(", ", protectionsSort.ProtectionsResolve.DisabledProtections.Select(p => p ?? "Unnamed Protection")));
            }
            if (protectionsSort.ProtectionsResolve.UnknownProtections.Any())
            {
                m_Logger.Warning("Unknown protections: {0}", string.Join(", ", protectionsSort.ProtectionsResolve.UnknownProtections.Select(p => p ?? "Unnamed Protection")));
            }
            if (protectionsSort.ObfuscationAttributeExcludeProtections.Any())
            {
                m_Logger.Warning("Skip protections with obfuscation attribute: {0}", string.Join(", ", protectionsSort.ObfuscationAttributeExcludeProtections.Select(p => p?.GetName())));
            }
        }
    }
}