using BitMono.API.Protecting.Contexts;
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

        public MethodImplAttributeExcludingResolver(IConfiguration configuration, IAttemptAttributeResolver attemptAttributeResolver)
        {
            m_Configuration = configuration;
            m_AttemptAttributeResolver = attemptAttributeResolver;
        }

        public bool TryResolve(ProtectionContext context, IDnlibDef dnlibDef, [AllowNull] out MethodImplAttribute methodImplAttribute, bool inherit = false)
        {
            var resolvingSucceed = m_AttemptAttributeResolver.TryResolve(context, dnlibDef, (obfuscationAttribute) =>
            {
                return m_Configuration.GetValue<bool>(nameof(AppSettings.NoInliningMethodObfuscationExcluding)) == true;
            }, null, out methodImplAttribute, inherit);

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