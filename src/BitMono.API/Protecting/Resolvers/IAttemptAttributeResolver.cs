namespace BitMono.API.Protecting.Resolvers;

public interface IAttemptAttributeResolver
{
    bool TryResolve(IHasCustomAttribute from, Type attributeType, out Dictionary<string, CustomAttributeResolve> keyValuePairs);
}