namespace BitMono.Core.Resolvers;

public class DoNotResolveMemberResolver : IMemberResolver
{
    private readonly RuntimeCriticalAnalyzer _runtimeCriticalAnalyzer;
    private readonly ModelAttributeCriticalAnalyzer _modelAttributeCriticalAnalyzer;
    private readonly ReflectionCriticalAnalyzer _reflectionCriticalAnalyzer;
    private readonly BamlCriticalAnalyzer _bamlCriticalAnalyzer;

    public DoNotResolveMemberResolver(
        RuntimeCriticalAnalyzer runtimeCriticalAnalyzer,
        ModelAttributeCriticalAnalyzer modelAttributeCriticalAnalyzer,
        ReflectionCriticalAnalyzer reflectionCriticalAnalyzer,
        BamlCriticalAnalyzer bamlCriticalAnalyzer)
    {
        _runtimeCriticalAnalyzer = runtimeCriticalAnalyzer;
        _modelAttributeCriticalAnalyzer = modelAttributeCriticalAnalyzer;
        _reflectionCriticalAnalyzer = reflectionCriticalAnalyzer;
        _bamlCriticalAnalyzer = bamlCriticalAnalyzer;
    }

    public bool Resolve(IProtection protection, IMetadataMember member)
    {
        if (!protection.TryGetDoNotResolveAttribute(out var doNotResolveAttribute))
        {
            return true;
        }
        if (doNotResolveAttribute!.MemberInclusion.HasFlag(MemberInclusionFlags.SpecialRuntime))
        {
            if (!_runtimeCriticalAnalyzer.NotCriticalToMakeChanges(member))
            {
                return false;
            }
        }
        if (member is IHasCustomAttribute customAttribute)
        {
            if (doNotResolveAttribute.MemberInclusion.HasFlag(MemberInclusionFlags.Model))
            {
                if (!_modelAttributeCriticalAnalyzer.NotCriticalToMakeChanges(customAttribute))
                {
                    return false;
                }
            }
        }
        if (doNotResolveAttribute.MemberInclusion.HasFlag(MemberInclusionFlags.Reflection))
        {
            switch (member)
            {
                case MethodDefinition method:
                    if (!_reflectionCriticalAnalyzer.NotCriticalToMakeChanges(method))
                    {
                        return false;
                    }
                    break;
                case FieldDefinition field:
                    if (!_reflectionCriticalAnalyzer.NotCriticalToMakeChanges(field))
                    {
                        return false;
                    }
                    break;
                case PropertyDefinition property:
                    if (!_reflectionCriticalAnalyzer.NotCriticalToMakeChanges(property))
                    {
                        return false;
                    }
                    break;
                case EventDefinition eventDef:
                    if (!_reflectionCriticalAnalyzer.NotCriticalToMakeChanges(eventDef))
                    {
                        return false;
                    }
                    break;
                case TypeDefinition type:
                    if (!_reflectionCriticalAnalyzer.NotCriticalToMakeChanges(type))
                    {
                        return false;
                    }
                    break;
            }
        }
        if (doNotResolveAttribute.MemberInclusion.HasFlag(MemberInclusionFlags.Baml))
        {
            if (!_bamlCriticalAnalyzer.NotCriticalToMakeChanges(member))
            {
                return false;
            }
        }
        return true;
    }
}