using BitMono.API.Configuration;
using BitMono.API.Protecting;
using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Injection;
using System.Threading;
using System.Threading.Tasks;

namespace BitMono.Protections
{
    public class AntiDe4dot : IProtection
    {
        private readonly IInjector m_Injector;

        public AntiDe4dot(IInjector injector, IBitMonoAppSettingsConfiguration bitMonoAppSettingsConfiguration)
        {
            m_Injector = injector;
        }

        public Task ExecuteAsync(ProtectionContext context, CancellationToken cancellationToken = default)
        {
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, "SmartAssembly.Attributes", "PoweredBy", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, "Xenocode.Client.Attributes.AssemblyAttributes", "PoweredBy", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, string.Empty, "ObfuscatedByGoliath", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, "SecureTeam.Attributes", "ObfuscatedByAgileDotNet", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, string.Empty, "TrinityObfuscator", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, "SecureTeam.Attributes", "ObfuscatedByCliSecure", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, string.Empty, "ZYXDNGuarder", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, string.Empty, "BabelObfuscator", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, string.Empty, "BabelObfuscator", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, string.Empty, "Dotfuscator", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, string.Empty, "Centos", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, string.Empty, "ConfusedBy", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, "NineRays.Obfuscator", "Evaluation", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, "CryptoObfuscator", "ProtectedWithCryptoObfuscator", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, string.Empty, "();\u0009", string.Empty);
            m_Injector.InjectAttributeWithContent(context.ModuleDefMD, string.Empty, "EMyPID_8234_", string.Empty);
            return Task.CompletedTask;
        }
    }
}