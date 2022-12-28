namespace BitMono.Core.Protecting.Resolvers;

public abstract class AttributeResolver : IAttributeResolver
{
    public abstract bool Resolve(string feature, IHasCustomAttribute from, out CustomAttributeResolve attributeResolve);
    public virtual bool Resolve(string feature, IHasCustomAttribute from)
    {
        return Resolve(feature, from, out _);
    }
    public virtual bool Resolve(IHasCustomAttribute from)
    {
        return Resolve(feature: null, from);
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