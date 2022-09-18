using BitMono.API.Injection;
using BitMono.API.Protections;
using BitMono.Core.Protections;
using System;
using System.Threading.Tasks;

namespace BitMono.Packers
{
    public class AntiDe4dot : IProtection, IAsyncDisposable
    {
        private readonly ProtectionContext m_Context;
        private readonly IInjector m_Injector;

        public AntiDe4dot(ProtectionContext context, IInjector injector)
        {
            m_Context = context;
            m_Injector = injector;
        }
        

        public Task ExecuteAsync()
        {
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, "SmartAssembly.Attributes", "PoweredBy", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, "Xenocode.Client.Attributes.AssemblyAttributes", "PoweredBy", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, string.Empty, "ObfuscatedByGoliath", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, "SecureTeam.Attributes", "ObfuscatedByAgileDotNet", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, string.Empty, "TrinityObfuscator", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, "SecureTeam.Attributes", "ObfuscatedByCliSecure", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, string.Empty, "ZYXDNGuarder", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, string.Empty, "BabelObfuscator", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, string.Empty, "BabelObfuscator", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, string.Empty, "Dotfuscator", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, string.Empty, "Centos", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, string.Empty, "ConfusedBy", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, "NineRays.Obfuscator", "Evaluation", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, "CryptoObfuscator", "ProtectedWithCryptoObfuscator", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, string.Empty, "();\u0009", string.Empty);
            m_Injector.InjectAttributeWithContent(m_Context.ModuleDefMD, string.Empty, "EMyPID_8234_", string.Empty);
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}