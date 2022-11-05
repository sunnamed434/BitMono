using dnlib.DotNet;
using System;
using System.Collections.Generic;

namespace BitMono.API.Protecting.Resolvers
{
    public interface ICustomAttributesResolver
    {
        IEnumerable<TAttribute> Resolve<TAttribute>(IHasCustomAttribute from) where TAttribute : Attribute;
    }
}