using BitMono.API.Protecting.Resolvers;
using dnlib.DotNet;
using NullGuard;
using System;
using System.Linq;

namespace BitMono.Core.Protecting.Resolvers
{
    public class AttemptAttributeResolver : IAttemptAttributeResolver
    {
        private readonly ICustomAttributesResolver m_CustomAttributesResolver;

        public AttemptAttributeResolver(ICustomAttributesResolver customAttributesResolver)
        {
            m_CustomAttributesResolver = customAttributesResolver;
        }

        public bool TryResolve<TAttribute>(IHasCustomAttribute from, [AllowNull] Func<TAttribute, bool> predicate, [AllowNull] Func<TAttribute, bool> strip, [AllowNull] out TAttribute attribute)
            where TAttribute : Attribute
        {
            attribute = predicate != null
                ? m_CustomAttributesResolver.Resolve(from, strip).FirstOrDefault(predicate)
                : m_CustomAttributesResolver.Resolve(from, strip).FirstOrDefault();
            if (attribute == null)
            {
                return false;
            }
            return true;
        }
    }
}