namespace BitMono.Core.Analyzing;

[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
[SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
public class ModelAttributeCriticalAnalyzer : ICriticalAnalyzer<IHasCustomAttribute>
{
    private readonly CriticalsSettings _criticalsSettings;

    public ModelAttributeCriticalAnalyzer(CriticalsSettings criticalsSettings)
    {
        _criticalsSettings = criticalsSettings;
    }

    public bool NotCriticalToMakeChanges(IHasCustomAttribute customAttribute)
    {
        if (_criticalsSettings.UseCriticalModelAttributes == false)
        {
            return true;
        }
        var criticalAttributes = _criticalsSettings.CriticalModelAttributes;
        for (var i = 0; i < criticalAttributes.Count; i++)
        {
            var attribute = criticalAttributes[i];
            if (AttemptAttributeResolver.TryResolve(customAttribute, attribute.Namespace, attribute.Name))
            {
                return false;
            }
        }
        return true;
    }
}