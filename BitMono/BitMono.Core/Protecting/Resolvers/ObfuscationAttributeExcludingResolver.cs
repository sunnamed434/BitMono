using BitMono.API.Protecting.Contexts;
using BitMono.API.Protecting.Resolvers;
using BitMono.Core.Models;
using dnlib.DotNet;
using Microsoft.Extensions.Configuration;
using NullGuard;
using System;
using System.Reflection;

namespace BitMono.Core.Protecting.Resolvers
{
    public class ObfuscationAttributeExcludingResolver : IObfuscationAttributeExcludingResolver
    {
        private readonly IAttemptAttributeResolver m_AttemptAttributeResolver;
        private readonly IConfiguration m_Configuration;

        public ObfuscationAttributeExcludingResolver(IConfiguration configuration, IAttemptAttributeResolver attemptAttributeResolver)
        {
            m_Configuration = configuration;
            m_AttemptAttributeResolver = attemptAttributeResolver;
        }

        public bool TryResolve(ProtectionContext context, IDnlibDef dnlibDef, string feature, [AllowNull] out ObfuscationAttribute obfuscationAttribute, bool inherit = false)
        {
            var resolvingSucceed = m_AttemptAttributeResolver.TryResolve(context, dnlibDef, (obfuscationAttribute) =>
            {
                return m_Configuration.GetValue<bool>(nameof(AppSettings.ObfuscationAttributeObfuscationExcluding)) == true;
            }, (o) => o.Feature.Equals(feature, StringComparison.OrdinalIgnoreCase), out obfuscationAttribute, inherit);

            if (resolvingSucceed == false)
            {
                return false;
            }
            if (obfuscationAttribute == null)
            {
                return false;
            }
            return true;
        }
    }
}