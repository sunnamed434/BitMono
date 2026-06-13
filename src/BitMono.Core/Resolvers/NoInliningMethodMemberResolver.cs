namespace BitMono.Core.Resolvers;

public class NoInliningMethodMemberResolver : IMemberResolver
{
    private readonly ObfuscationSettings _obfuscationSettings;

    public NoInliningMethodMemberResolver(ObfuscationSettings obfuscationSettings)
    {
        _obfuscationSettings = obfuscationSettings;
    }

    public bool Resolve(IProtection? protection, IMetadataMember member)
    {
        if (!_obfuscationSettings.NoInliningMethodObfuscationExclude)
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
