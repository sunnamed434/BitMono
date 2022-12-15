namespace BitMono.API.Protecting.Resolvers;

public interface IDnlibDefObfuscationAttributeResolver
{
    bool Resolve(string feature, IDnlibDef dnlibDef);
    bool Resolve<TFeature>(IDnlibDef dnlibDef) where TFeature : IProtection;
}