namespace BitMono.Protections;

[DoNotResolve(Members.SpecialRuntime | Members.Model)]
public class FullRenamer : IProtection
{
    private readonly RuntimeCriticalAnalyzer m_RuntimeCriticalAnalyzer;
    private readonly ModelAttributeCriticalAnalyzer m_ModelAttributelCriticalAnalyzer;
    private readonly IRenamer m_Renamer;

    public FullRenamer(RuntimeCriticalAnalyzer runtimeCriticalAnalyzer, ModelAttributeCriticalAnalyzer modelAttributeCriticalAnalyzer, IRenamer renamer)
    {
        m_RuntimeCriticalAnalyzer = runtimeCriticalAnalyzer;
        m_ModelAttributelCriticalAnalyzer = modelAttributeCriticalAnalyzer;
        m_Renamer = renamer;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default)
    {
        var moduleType = context.Module.GetOrCreateModuleType();
        foreach (var type in parameters.Targets.OfType<TypeDefinition>())
        {
            Console.WriteLine("-------------------> PREPARE TO RENAME THE TYPE: " + type.FullName);
            if (type != moduleType
                && m_RuntimeCriticalAnalyzer.NotCriticalToMakeChanges(type)
                && m_ModelAttributelCriticalAnalyzer.NotCriticalToMakeChanges(type))
            {
                Console.WriteLine("-------------------> RENAME TYPE: " + type.FullName);
                m_Renamer.Rename(type);
                foreach (var field in type.Fields.ToArray())
                {
                    if (m_RuntimeCriticalAnalyzer.NotCriticalToMakeChanges(field))
                    {
                        Console.WriteLine("-------------------> RENAME field: " + field.FullName);
                        m_Renamer.Rename(field);
                    }
                }
                foreach (var method in type.Methods.ToArray())
                {
                    if (method.IsConstructor == false
                        && method.IsVirtual == false
                        && m_RuntimeCriticalAnalyzer.NotCriticalToMakeChanges(method))
                    {
                        Console.WriteLine("-------------------> RENAME method: " + method.FullName);
                        m_Renamer.Rename(method);
                        if (method.HasParameters())
                        {
                            foreach (var parameter in method.Parameters.ToArray())
                            {
                                Console.WriteLine("-------------------> RENAME parameter: " + parameter.Name);
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