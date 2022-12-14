namespace BitMono.Protections;

public class AntiDe4dot : IProtection
{
    private readonly CustomInjector m_Injector;

    public AntiDe4dot(CustomInjector injector)
    {
        m_Injector = injector;
    }

    public Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters)
    {
        m_Injector.InjectAttribute(context.Module, "SmartAssembly.Attributes", "PoweredBy", string.Empty);
        m_Injector.InjectAttribute(context.Module, "Xenocode.Client.Attributes.AssemblyAttributes", "PoweredBy", string.Empty);
        m_Injector.InjectAttribute(context.Module, string.Empty, "ObfuscatedByGoliath", string.Empty);
        m_Injector.InjectAttribute(context.Module, "SecureTeam.Attributes", "ObfuscatedByAgileDotNet", string.Empty);
        m_Injector.InjectAttribute(context.Module, string.Empty, "TrinityObfuscator", string.Empty);
        m_Injector.InjectAttribute(context.Module, "SecureTeam.Attributes", "ObfuscatedByCliSecure", string.Empty);
        m_Injector.InjectAttribute(context.Module, string.Empty, "ZYXDNGuarder", string.Empty);
        m_Injector.InjectAttribute(context.Module, string.Empty, "BabelObfuscator", string.Empty);
        m_Injector.InjectAttribute(context.Module, string.Empty, "BabelObfuscator", string.Empty);
        m_Injector.InjectAttribute(context.Module, string.Empty, "Dotfuscator", string.Empty);
        m_Injector.InjectAttribute(context.Module, string.Empty, "Centos", string.Empty);
        m_Injector.InjectAttribute(context.Module, string.Empty, "ConfusedBy", string.Empty);
        m_Injector.InjectAttribute(context.Module, "NineRays.Obfuscator", "Evaluation", string.Empty);
        m_Injector.InjectAttribute(context.Module, "CryptoObfuscator", "ProtectedWithCryptoObfuscator", string.Empty);
        m_Injector.InjectAttribute(context.Module, string.Empty, "();\u0009", string.Empty);
        m_Injector.InjectAttribute(context.Module, string.Empty, "EMyPID_8234_", string.Empty);
        return Task.CompletedTask;
    }
}