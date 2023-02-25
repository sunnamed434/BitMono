namespace BitMono.Protections;

public class AntiDe4dot : Protection
{
    private readonly CustomInjector m_Injector;

    public AntiDe4dot(CustomInjector injector, ProtectionContext context) : base(context)
    {
        m_Injector = injector;
    }

    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        m_Injector.InjectAttribute(Context.Module, "SmartAssembly.Attributes", "PoweredBy", string.Empty);
        m_Injector.InjectAttribute(Context.Module, "Xenocode.Client.Attributes.AssemblyAttributes", "PoweredBy", string.Empty);
        m_Injector.InjectAttribute(Context.Module, string.Empty, "ObfuscatedByGoliath", string.Empty);
        m_Injector.InjectAttribute(Context.Module, "SecureTeam.Attributes", "ObfuscatedByAgileDotNet", string.Empty);
        m_Injector.InjectAttribute(Context.Module, string.Empty, "TrinityObfuscator", string.Empty);
        m_Injector.InjectAttribute(Context.Module, "SecureTeam.Attributes", "ObfuscatedByCliSecure", string.Empty);
        m_Injector.InjectAttribute(Context.Module, string.Empty, "ZYXDNGuarder", string.Empty);
        m_Injector.InjectAttribute(Context.Module, string.Empty, "BabelObfuscator", string.Empty);
        m_Injector.InjectAttribute(Context.Module, string.Empty, "BabelObfuscator", string.Empty);
        m_Injector.InjectAttribute(Context.Module, string.Empty, "Dotfuscator", string.Empty);
        m_Injector.InjectAttribute(Context.Module, string.Empty, "Centos", string.Empty);
        m_Injector.InjectAttribute(Context.Module, string.Empty, "ConfusedBy", string.Empty);
        m_Injector.InjectAttribute(Context.Module, "NineRays.Obfuscator", "Evaluation", string.Empty);
        m_Injector.InjectAttribute(Context.Module, "CryptoObfuscator", "ProtectedWithCryptoObfuscator", string.Empty);
        m_Injector.InjectAttribute(Context.Module, string.Empty, "();\u0009", string.Empty);
        m_Injector.InjectAttribute(Context.Module, string.Empty, "EMyPID_8234_", string.Empty);
        return Task.CompletedTask;
    }
}