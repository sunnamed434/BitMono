using BitMono.API.Protecting.Contexts;
using dnlib.DotNet;
using System.Reflection;

namespace BitMono.API.Protecting.Resolvers
{
    public interface IObfuscationAttributeExcludingResolver
    {
        bool TryResolve(ProtectionContext context, IDnlibDef dnlibDef, string feature, out ObfuscationAttribute obfuscationAttribute, bool inherit = false);
    }
}