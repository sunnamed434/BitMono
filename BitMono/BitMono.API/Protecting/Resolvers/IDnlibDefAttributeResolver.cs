using BitMono.API.Protecting.Contexts;
using dnlib.DotNet;
using System;

namespace BitMono.API.Protecting.Resolvers
{
    public interface IDnlibDefAttributeResolver
    {
        TAttribute ResolveOrDefault<TAttribute>(ProtectionContext context, IDnlibDef dnlibDef, Func<TAttribute, bool> predicate = default, bool inherit = false)
            where TAttribute : Attribute;
    }
}