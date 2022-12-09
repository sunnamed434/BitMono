using dnlib.DotNet;
using System.Reflection;

namespace BitMono.API.Protecting.Resolvers
{
    public interface IObfuscationAttributeExcludingResolver
    {
        bool TryResolve(IHasCustomAttribute from, string feature, out ObfuscationAttribute obfuscationAttribute);
    }
}