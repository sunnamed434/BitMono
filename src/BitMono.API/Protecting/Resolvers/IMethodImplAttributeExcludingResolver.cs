namespace BitMono.API.Protecting.Resolvers;

public interface IMethodImplAttributeExcludingResolver
{
    bool TryResolve(IHasCustomAttribute from, out MethodImplAttribute obfuscationAttribute);
}