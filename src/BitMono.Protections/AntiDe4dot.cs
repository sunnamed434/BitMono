namespace BitMono.Protections;

public class AntiDe4dot : Protection
{
    public AntiDe4dot(ProtectionContext context) : base(context)
    {
    }

    public override Task ExecuteAsync(ProtectionParameters parameters)
    {
        CustomInjector.InjectAttribute(Context.Module, "SmartAssembly.Attributes", "PoweredBy", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, "Xenocode.Client.Attributes.AssemblyAttributes", "PoweredBy", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, string.Empty, "ObfuscatedByGoliath", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, "SecureTeam.Attributes", "ObfuscatedByAgileDotNet", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, string.Empty, "TrinityObfuscator", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, "SecureTeam.Attributes", "ObfuscatedByCliSecure", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, string.Empty, "ZYXDNGuarder", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, string.Empty, "BabelObfuscator", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, string.Empty, "BabelObfuscator", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, string.Empty, "Dotfuscator", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, string.Empty, "Centos", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, string.Empty, "ConfusedBy", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, "NineRays.Obfuscator", "Evaluation", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, "CryptoObfuscator", "ProtectedWithCryptoObfuscator", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, string.Empty, "();\u0009", string.Empty);
        CustomInjector.InjectAttribute(Context.Module, string.Empty, "EMyPID_8234_", string.Empty);
        return Task.CompletedTask;
    }
}