namespace BitMono.Protections;

[DoNotResolve(MemberInclusionFlags.SpecialRuntime | MemberInclusionFlags.Model | MemberInclusionFlags.Reflection)]
public class FullRenamer : IProtection
{
    private readonly IRenamer m_Renamer;

    public FullRenamer(IRenamer renamer)
    {
        m_Renamer = renamer;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        foreach (var method in parameters.Members.OfType<MethodDefinition>())
        {
            if (method.DeclaringType.IsModuleType == false && method.IsConstructor == false && method.IsVirtual == false)
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
        foreach (var type in parameters.Members.OfType<TypeDefinition>())
        {
            if (type.IsModuleType == false)
            {
                m_Renamer.Rename(type);
            }
        }
        foreach (var field in parameters.Members.OfType<FieldDefinition>())
        {
            if (field.DeclaringType.IsModuleType == false)
            {
                m_Renamer.Rename(field);
            }
        }
        return Task.CompletedTask;
    }
}