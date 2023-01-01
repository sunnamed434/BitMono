namespace BitMono.Core.Protecting.Resolvers;

public class DoNotResolveMemberResolver : IMemberResolver
{
    private readonly RuntimeCriticalAnalyzer m_RuntimeCriticalAnalyzer;
    private readonly ModelAttributeCriticalAnalyzer m_ModelAttributeCriticalAnalyzer;

    public DoNotResolveMemberResolver(RuntimeCriticalAnalyzer runtimeCriticalAnalyzer, ModelAttributeCriticalAnalyzer modelAttributeCriticalAnalyzer)
    {
        m_RuntimeCriticalAnalyzer = runtimeCriticalAnalyzer;
        m_ModelAttributeCriticalAnalyzer = modelAttributeCriticalAnalyzer;
    }

    public bool Resolve(IProtection protection, IMetadataMember member)
    {
        if (protection.TryGetDoNotResolveAttribute(out DoNotResolveAttribute doNotResolveAttribute) == false)
        {
            return true;
        }
        if (doNotResolveAttribute.Members.HasFlag(Members.SpecialRuntime))
        {
            if (m_RuntimeCriticalAnalyzer.NotCriticalToMakeChanges(member) == false)
            {
                return false;
            }
        }
        if (member is IHasCustomAttribute customAttribute)
        {
            if (doNotResolveAttribute.Members.HasFlag(Members.Model))
            {
                if (m_ModelAttributeCriticalAnalyzer.NotCriticalToMakeChanges(customAttribute) == false)
                {
                    return false;
                }
            }
        }
        return true;
    }
}