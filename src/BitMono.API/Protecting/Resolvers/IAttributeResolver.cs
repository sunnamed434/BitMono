namespace BitMono.API.Protecting.Resolvers;

public interface IAttributeResolver
{
    bool Resolve(string feature, IHasCustomAttribute from, out CustomAttributeResolve attributeResolve);
    bool Resolve(string feature, IHasCustomAttribute from);
    bool Resolve(IHasCustomAttribute from);
    bool Resolve(Type featureType, IHasCustomAttribute from);
    bool Resolve<TFeature>(IHasCustomAttribute from) where TFeature : IProtection;
}