namespace BitMono.API.Protecting.Resolvers;

public interface ICustomAttributeResolver
{
    Dictionary<string, CustomAttributesResolve> Resolve(IHasCustomAttribute from, Type attributeType);
}