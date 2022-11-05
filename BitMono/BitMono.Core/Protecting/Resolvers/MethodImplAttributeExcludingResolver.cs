using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Models;
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

        public MethodImplAttributeExcludingResolver(IAttemptAttributeResolver attemptAttributeResolver, IConfiguration configuration)
        {
            m_AttemptAttributeResolver = attemptAttributeResolver;
            m_Configuration = configuration;
        }

        public bool TryResolve(IHasCustomAttribute from, [AllowNull] out MethodImplAttribute methodImplAttribute)
        {
            var resolvingSucceed = m_AttemptAttributeResolver.TryResolve(from, (obfuscationAttribute) =>
            {
                return m_Configuration.GetValue<bool>(nameof(AppSettings.NoInliningMethodObfuscationExcluding)) == true;
            }, null, out methodImplAttribute);

            if (resolvingSucceed == false)
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