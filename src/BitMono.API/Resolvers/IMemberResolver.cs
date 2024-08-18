namespace BitMono.API.Resolvers;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IMemberResolver
{
    bool Resolve(IProtection protection, IMetadataMember member);
}