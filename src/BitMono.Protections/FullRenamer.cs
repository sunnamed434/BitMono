namespace BitMono.Protections;

[DoNotResolve(Members.SpecialRuntime | Members.Model)]
public class FullRenamer : IProtection
{
    private readonly IRenamer m_Renamer;

    public FullRenamer(IRenamer renamer)
    {
        m_Renamer = renamer;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        var moduleType = context.Module.GetOrCreateModuleType();
        foreach (var type in parameters.Members.OfType<TypeDefinition>())
        {
            if (type != moduleType)
            {
                m_Renamer.Rename(type);
                foreach (var field in type.Fields)
                {
                    m_Renamer.Rename(field);
                }
                foreach (var method in type.Methods)
                {
                    if (method.IsConstructor == false && method.IsVirtual == false)
                    {
                        m_Renamer.Rename(method);
                        if (method.HasParameters())
                        {
                            foreach (var parameter in method.Parameters)
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