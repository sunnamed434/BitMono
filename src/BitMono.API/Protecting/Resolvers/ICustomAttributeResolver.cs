namespace BitMono.API.Protecting.Resolvers;

public interface ICustomAttributeResolver
{
    Dictionary<string, CustomAttributeResolve> Resolve(IHasCustomAttribute from, string @namespace, string name);
}