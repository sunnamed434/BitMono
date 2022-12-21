namespace BitMono.Protections;

public class FullRenamer : IProtection
{
    private readonly DnlibDefCriticalAnalyzer m_DnlibDefCriticalAnalyzer;
    private readonly TypeDefAttributeCriticalAnalyzer m_TypeDefModelCriticalAnalyzer;
    private readonly IRenamer m_Renamer;

    public FullRenamer(DnlibDefCriticalAnalyzer dnlibDefCriticalAnalyzer, TypeDefAttributeCriticalAnalyzer typeDefModelCriticalAnalyzer, IRenamer renamer)
    {
        m_DnlibDefCriticalAnalyzer = dnlibDefCriticalAnalyzer;
        m_TypeDefModelCriticalAnalyzer = typeDefModelCriticalAnalyzer;
        m_Renamer = renamer;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        var moduleType = context.Module.GetOrCreateModuleType();
        foreach (var type in parameters.Targets.OfType<TypeDefinition>())
        {
            if (type != moduleType
                && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(type)
                && m_TypeDefModelCriticalAnalyzer.NotCriticalToMakeChanges(type))
            {
                m_Renamer.Rename(type);

                foreach (var field in type.Fields.ToArray())
                {
                    if (m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(field))
                    {
                        m_Renamer.Rename(field);
                    }
                }
                foreach (var method in type.Methods.ToArray())
                {
                    if (method.IsConstructor == false
                        && method.IsVirtual == false
                        && m_DnlibDefCriticalAnalyzer.NotCriticalToMakeChanges(method))
                    {
                        m_Renamer.Rename(method);
                        if (method.HasParameters())
                        {
                            foreach (var parameter in method.Parameters.ToArray())
                            {
                                m_Renamer.Rename(parameter.Definition);
                            }
                        }
                    }
                }
            }
        }
        return Task.CompletedTask;
    }
}