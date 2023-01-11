namespace BitMono.Core.Protecting.Resolvers;

public class NoInliningMethodMemberResolver : IMemberResolver
{
    private readonly IConfiguration m_Configuration;

    public NoInliningMethodMemberResolver(IBitMonoObfuscationConfiguration configuration)
    {
        m_Configuration = configuration.Configuration;
    }

    public bool Resolve(IProtection protection, IMetadataMember member)
    {
        if (m_Configuration.GetValue<bool>(nameof(Obfuscation.NoInliningMethodObfuscationExclude)) == false)
        {
            return true;
        }
        if (member is MethodDefinition method)
        {
            if (method.NoInlining)
            {
                return false;
            }
        }
        return true;
    }
}
