namespace BitMono.Core.Resolvers;

public static class AttemptAttributeResolver
{
    public static bool TryResolve(IHasCustomAttribute from, string @namespace, string name, out List<CustomAttributeResolve>? attributesResolve)
    {
        attributesResolve = CustomAttributeResolver.Resolve(from, @namespace, name);
        return attributesResolve.IsNullOrEmpty() == false;
    }
    public static bool TryResolve(IHasCustomAttribute from, string @namespace, string name)
    {
        return TryResolve(from, @namespace, name, out _);
    }
}