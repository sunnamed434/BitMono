namespace BitMono.Core.Resolvers;

public class DoNotResolveMemberResolver : IMemberResolver
{
    private readonly RuntimeCriticalAnalyzer _runtimeCriticalAnalyzer;
    private readonly ModelAttributeCriticalAnalyzer _modelAttributeCriticalAnalyzer;
    private readonly ReflectionCriticalAnalyzer _reflectionCriticalAnalyzer;

    public DoNotResolveMemberResolver(
        RuntimeCriticalAnalyzer runtimeCriticalAnalyzer,
        ModelAttributeCriticalAnalyzer modelAttributeCriticalAnalyzer,
        ReflectionCriticalAnalyzer reflectionCriticalAnalyzer)
    {
        _runtimeCriticalAnalyzer = runtimeCriticalAnalyzer;
        _modelAttributeCriticalAnalyzer = modelAttributeCriticalAnalyzer;
        _reflectionCriticalAnalyzer = reflectionCriticalAnalyzer;
    }

    public bool Resolve(IProtection protection, IMetadataMember member)
    {
        if (protection.TryGetDoNotResolveAttribute(out var doNotResolveAttribute) == false)
        {
            return true;
        }
        if (doNotResolveAttribute!.MemberInclusion.HasFlag(MemberInclusionFlags.SpecialRuntime))
        {
            if (_runtimeCriticalAnalyzer.NotCriticalToMakeChanges(member) == false)
            {
                return false;
            }
        }
        if (member is IHasCustomAttribute customAttribute)
        {
            if (doNotResolveAttribute.MemberInclusion.HasFlag(MemberInclusionFlags.Model))
            {
                if (_modelAttributeCriticalAnalyzer.NotCriticalToMakeChanges(customAttribute) == false)
                {
                    return false;
                }
            }
        }
        if (member is MethodDefinition method)
        {
            if (doNotResolveAttribute.MemberInclusion.HasFlag(MemberInclusionFlags.Reflection))
            {
                if (_reflectionCriticalAnalyzer.NotCriticalToMakeChanges(method) == false)
                {
                    return false;
                }
            }
        }
        return true;
    }
}