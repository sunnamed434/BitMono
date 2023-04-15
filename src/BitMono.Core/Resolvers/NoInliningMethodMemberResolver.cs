using BitMono.API.Resolvers;

namespace BitMono.Core.Resolvers;

public class NoInliningMethodMemberResolver : IMemberResolver
{
    private readonly ObfuscationSettings _obfuscationSettings;

    public NoInliningMethodMemberResolver(IOptions<ObfuscationSettings> obfuscation)
    {
        _obfuscationSettings = obfuscation.Value;
    }

    public bool Resolve(IProtection? protection, IMetadataMember member)
    {
        if (_obfuscationSettings.NoInliningMethodObfuscationExclude == false)
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
