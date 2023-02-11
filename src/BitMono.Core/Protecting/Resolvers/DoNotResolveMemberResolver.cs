namespace BitMono.Core.Protecting.Resolvers;

public class DoNotResolveMemberResolver : IMemberResolver
{
    private readonly RuntimeCriticalAnalyzer m_RuntimeCriticalAnalyzer;
    private readonly ModelAttributeCriticalAnalyzer m_ModelAttributeCriticalAnalyzer;
    private readonly ReflectionCriticalAnalyzer m_ReflectionCriticalAnalyzer;

    public DoNotResolveMemberResolver(
        RuntimeCriticalAnalyzer runtimeCriticalAnalyzer,
        ModelAttributeCriticalAnalyzer modelAttributeCriticalAnalyzer,
        ReflectionCriticalAnalyzer reflectionCriticalAnalyzer)
    {
        m_RuntimeCriticalAnalyzer = runtimeCriticalAnalyzer;
        m_ModelAttributeCriticalAnalyzer = modelAttributeCriticalAnalyzer;
        m_ReflectionCriticalAnalyzer = reflectionCriticalAnalyzer;
    }

    public bool Resolve(IProtection protection, IMetadataMember member)
    {
        if (protection.TryGetDoNotResolveAttribute(out var doNotResolveAttribute) == false)
        {
            return true;
        }
        if (doNotResolveAttribute.MemberInclusion.HasFlag(MemberInclusionFlags.SpecialRuntime))
        {
            if (m_RuntimeCriticalAnalyzer.NotCriticalToMakeChanges(member) == false)
            {
                return false;
            }
        }
        if (member is IHasCustomAttribute customAttribute)
        {
            if (doNotResolveAttribute.MemberInclusion.HasFlag(MemberInclusionFlags.Model))
            {
                if (m_ModelAttributeCriticalAnalyzer.NotCriticalToMakeChanges(customAttribute) == false)
                {
                    return false;
                }
            }
        }
        if (member is MethodDefinition method)
        {
            if (doNotResolveAttribute.MemberInclusion.HasFlag(MemberInclusionFlags.Reflection))
            {
                if (m_ReflectionCriticalAnalyzer.NotCriticalToMakeChanges(method) == false)
                {
                    return false;
                }
            }
        }
        return true;
    }
}