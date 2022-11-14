using dnlib.DotNet;

namespace BitMono.API.Protecting.Resolvers
{
    public interface IDnlibDefFeatureObfuscationAttributeHavingResolver
    {
        bool Resolve(IDnlibDef dnlibDef, string feature);
        bool Resolve<TFeature>(IDnlibDef dnlibDef) where TFeature : IProtection;
    }
}