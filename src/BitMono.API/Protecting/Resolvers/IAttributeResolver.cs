namespace BitMono.API.Protecting.Resolvers;

public interface IAttributeResolver
{
    bool Resolve([AllowNull] string feature, IHasCustomAttribute from, [AllowNull] out CustomAttributeResolve attributeResolve);
    bool Resolve([AllowNull] string feature, IHasCustomAttribute from);
    bool Resolve(IHasCustomAttribute from);
    bool Resolve(Type featureType, IHasCustomAttribute from);
    bool Resolve<TFeature>(IHasCustomAttribute from) where TFeature : IProtection;
}