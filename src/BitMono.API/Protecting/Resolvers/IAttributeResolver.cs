namespace BitMono.API.Protecting.Resolvers;

public interface IAttributeResolver<TModel>
    where TModel : class
{
    bool Resolve(string? featureName, IHasCustomAttribute from, out TModel? model);
    bool Resolve(string? featureName, IHasCustomAttribute from);
    bool Resolve(IHasCustomAttribute from);
    bool Resolve(Type featureType, IHasCustomAttribute from);
    bool Resolve<TFeature>(IHasCustomAttribute from) where TFeature : IProtection;
}