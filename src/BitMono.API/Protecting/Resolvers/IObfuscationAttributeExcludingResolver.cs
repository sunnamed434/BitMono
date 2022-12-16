namespace BitMono.API.Protecting.Resolvers;

public interface IObfuscationAttributeExcludingResolver
{
    bool TryResolve(string feature, IHasCustomAttribute from, out ObfuscationAttribute obfuscationAttribute);
}