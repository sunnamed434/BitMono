namespace BitMono.Protections;

public class AntiDe4dot : Protection
{
    public AntiDe4dot(IBitMonoServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override Task ExecuteAsync()
    {
        var module = Context.Module;
        CustomInjector.InjectAttribute(module, "SmartAssembly.Attributes", "PoweredBy", string.Empty);
        CustomInjector.InjectAttribute(module, "Xenocode.Client.Attributes.AssemblyAttributes", "PoweredBy", string.Empty);
        CustomInjector.InjectAttribute(module, string.Empty, "ObfuscatedByGoliath", string.Empty);
        CustomInjector.InjectAttribute(module, "SecureTeam.Attributes", "ObfuscatedByAgileDotNet", string.Empty);
        CustomInjector.InjectAttribute(module, string.Empty, "TrinityObfuscator", string.Empty);
        CustomInjector.InjectAttribute(module, "SecureTeam.Attributes", "ObfuscatedByCliSecure", string.Empty);
        CustomInjector.InjectAttribute(module, string.Empty, "ZYXDNGuarder", string.Empty);
        CustomInjector.InjectAttribute(module, string.Empty, "BabelObfuscator", string.Empty);
        CustomInjector.InjectAttribute(module, string.Empty, "BabelObfuscator", string.Empty);
        CustomInjector.InjectAttribute(module, string.Empty, "Dotfuscator", string.Empty);
        CustomInjector.InjectAttribute(module, string.Empty, "Centos", string.Empty);
        CustomInjector.InjectAttribute(module, string.Empty, "ConfusedBy", string.Empty);
        CustomInjector.InjectAttribute(module, "NineRays.Obfuscator", "Evaluation", string.Empty);
        CustomInjector.InjectAttribute(module, "CryptoObfuscator", "ProtectedWithCryptoObfuscator", string.Empty);
        CustomInjector.InjectAttribute(module, string.Empty, "();\u0009", string.Empty);
        CustomInjector.InjectAttribute(module, string.Empty, "EMyPID_8234_", string.Empty);
        return Task.CompletedTask;
    }
}