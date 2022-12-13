using dnlib.DotNet;

namespace BitMono.API.Protecting.Resolvers
{
    public interface IDnlibDefResolver
    {
        bool Resolve(string feature, IDnlibDef dnlibDef);
    }
}