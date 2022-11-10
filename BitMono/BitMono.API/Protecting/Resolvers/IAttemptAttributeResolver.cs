using dnlib.DotNet;
using System;

namespace BitMono.API.Protecting.Resolvers
{
    public interface IAttemptAttributeResolver
    {
        bool TryResolve<TAttribute>(IHasCustomAttribute from, Func<TAttribute, bool> predicate, Func<TAttribute, bool> strip, out TAttribute attribute)
            where TAttribute : Attribute;
    }
}