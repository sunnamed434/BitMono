namespace BitMono.Core.Protecting.Resolvers;

public class AttributeResolver : IAttributeResolver
{
    public virtual bool Resolve([AllowNull] string feature, IHasCustomAttribute from, [AllowNull] out CustomAttributeResolve attributeResolve)
    {
        attributeResolve = null;
        return false;
    }
    public virtual bool Resolve([AllowNull] string feature, IHasCustomAttribute from)
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