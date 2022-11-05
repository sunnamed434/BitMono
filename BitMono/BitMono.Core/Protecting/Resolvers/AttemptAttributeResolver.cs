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

        public bool TryResolve<TAttribute>(IHasCustomAttribute from, [AllowNull] Predicate<TAttribute> review, [AllowNull] Func<TAttribute, bool> predicate, [AllowNull] out TAttribute attribute)
            where TAttribute : Attribute
        {
            attribute = m_CustomAttributesResolver.Resolve<TAttribute>(from).FirstOrDefault(predicate);
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