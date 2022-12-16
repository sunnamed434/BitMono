namespace BitMono.API.Protecting.Resolvers;

public interface IDnlibDefResolver
{
    bool Resolve(string feature, IDnlibDef dnlibDef);
}