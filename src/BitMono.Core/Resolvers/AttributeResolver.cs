namespace BitMono.Core.Resolvers;

public class AttributeResolver<TModel> : IAttributeResolver<TModel> where TModel : class
{
    public virtual bool Resolve(string? featureName, IHasCustomAttribute from, out TModel? model)
    {
        model = default;
        return false;
    }
    public virtual bool Resolve(string? featureName, IHasCustomAttribute from)
    {
        return Resolve(featureName, from, out _);
    }
    public virtual bool Resolve(IHasCustomAttribute from)
    {
        return Resolve(featureName: null, from);
    }
    public virtual bool Resolve(Type featureType, IHasCustomAttribute from)
    {
        return Resolve(featureType.GetName(), from);
    }
    public virtual bool Resolve<TFeature>(IHasCustomAttribute from) where TFeature : IProtection
    {
        return Resolve(typeof(TFeature), from);
    }
}