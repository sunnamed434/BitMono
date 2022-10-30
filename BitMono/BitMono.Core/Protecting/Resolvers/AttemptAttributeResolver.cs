using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Resolvers;
using dnlib.DotNet;
using NullGuard;
using System;

namespace BitMono.Core.Protecting.Resolvers
{
    public class AttemptAttributeResolver : IAttemptAttributeResolver
    {
        private readonly IDnlibDefAttributeResolver m_DnlibDefAttributeResolver;

        public AttemptAttributeResolver(IDnlibDefAttributeResolver dnlibDefAttributeResolver)
        {
            m_DnlibDefAttributeResolver = dnlibDefAttributeResolver;
        }

        public bool TryResolve<TAttribute>(ProtectionContext context, IDnlibDef dnlibDef, [AllowNull] Predicate<TAttribute> review, [AllowNull] Func<TAttribute, bool> predicate, [AllowNull] out TAttribute attribute, bool inherit = false)
            where TAttribute : Attribute
        {
            attribute = m_DnlibDefAttributeResolver.ResolveOrDefault(context, dnlibDef, predicate, inherit);
            if (attribute == null)
            {
                return false;
            }
            if (review != null)
            {
                return review.Invoke(attribute);
            }
            return true;
        }
    }
}