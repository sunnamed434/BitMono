using BitMono.API.Protecting.Contexts;
using dnlib.DotNet;
using System;

namespace BitMono.API.Protecting.Resolvers
{
    public interface IAttemptAttributeResolver
    {
        bool TryResolve<TAttribute>(ProtectionContext context, IDnlibDef dnlibDef, Predicate<TAttribute> review, Func<TAttribute, bool> predicate, out TAttribute attribute, bool inherit = false)
            where TAttribute : Attribute;
    }
}