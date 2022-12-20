namespace BitMono.API.Protecting.Resolvers;

public interface IObfuscationAttributeResolver
{
    bool Resolve(string feature, IHasCustomAttribute from);
    bool Resolve<TFeature>(IHasCustomAttribute from) where TFeature : IProtection;
}