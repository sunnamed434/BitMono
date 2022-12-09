using BitMono.API.Configuration;
using BitMono.API.Protecting.Resolvers;
using BitMono.Shared.Models;
using dnlib.DotNet;
using Microsoft.Extensions.Configuration;
using NullGuard;
using System.Runtime.CompilerServices;

namespace BitMono.Core.Protecting.Resolvers
{
    public class MethodImplAttributeExcludingResolver : IMethodImplAttributeExcludingResolver
    {
        private readonly IAttemptAttributeResolver m_AttemptAttributeResolver;
        private readonly IConfiguration m_Configuration;

        public MethodImplAttributeExcludingResolver(IAttemptAttributeResolver attemptAttributeResolver, IBitMonoObfuscationConfiguration configuration)
        {
            m_AttemptAttributeResolver = attemptAttributeResolver;
            m_Configuration = configuration.Configuration;
        }

        public bool TryResolve(IHasCustomAttribute from, [AllowNull] out MethodImplAttribute methodImplAttribute)
        {
            methodImplAttribute = null;
            if (m_Configuration.GetValue<bool>(nameof(Obfuscation.NoInliningMethodObfuscationExcluding)) == false)
            {
                return false;
            }
            if (m_AttemptAttributeResolver.TryResolve(from, null, null, out methodImplAttribute) == false)
            {
                return false;
            }
            if (methodImplAttribute == null)
            {
                return false;
            }
            if (methodImplAttribute.Value == MethodImplOptions.NoInlining)
            {
                return false;
            }
            return true;
        }
    }
}