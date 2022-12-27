namespace BitMono.API.Protecting.Resolvers;

public interface IMemberResolver
{
    bool Resolve(string feature, IMetadataMember member);
}