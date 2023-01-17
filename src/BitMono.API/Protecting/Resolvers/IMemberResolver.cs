namespace BitMono.API.Protecting.Resolvers;

public interface IMemberResolver
{
    bool Resolve(IProtection protection, IMetadataMember member);
}