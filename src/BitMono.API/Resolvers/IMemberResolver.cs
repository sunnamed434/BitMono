namespace BitMono.API.Resolvers;

public interface IMemberResolver
{
    bool Resolve(IProtection protection, IMetadataMember member);
}