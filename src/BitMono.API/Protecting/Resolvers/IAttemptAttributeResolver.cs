namespace BitMono.API.Protecting.Resolvers;

public interface IAttemptAttributeResolver
{
    bool TryResolve(IHasCustomAttribute from, string @namespace, string name, out Dictionary<string, CustomAttributeResolve> keyValuePairs);
}