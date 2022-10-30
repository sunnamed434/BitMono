using BitMono.API.Protecting.Contexts;
using dnlib.DotNet;
using System.Runtime.CompilerServices;

namespace BitMono.API.Protecting.Resolvers
{
    public interface IMethodImplAttributeExcludingResolver
    {
        bool TryResolve(ProtectionContext context, IDnlibDef dnlibDef, out MethodImplAttribute obfuscationAttribute, bool inherit = false);
    }
}