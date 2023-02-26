#nullable enable
namespace BitMono.Host.Extensions;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public static class AutofacServiceProviderExtensions
{
    private static readonly string ProtectionsFileName = "BitMono.Protections.dll";
    public static ContainerBuilder AddProtections(this ContainerBuilder source, string? file = null)
    {
        var rawData = File.ReadAllBytes(file ?? ProtectionsFileName);
        Assembly.Load(rawData);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        source.RegisterAssemblyTypes(assemblies)
            .PublicOnly()
            .Where(t => t.GetInterface(nameof(IPhaseProtection)) == null && t.GetInterface(nameof(IProtection)) != null)
            .OwnedByLifetimeScope()
            .AsImplementedInterfaces()
            .SingleInstance();
        return source;
    }
    public static ServiceCollection AddConfigurations(this ServiceCollection source,
        string? protectionsFile = null, string? criticalsFile = null, string? obfuscationFile = null)
    {
        var protections = new BitMonoProtectionsConfiguration(protectionsFile);
        var criticals = new BitMonoCriticalsConfiguration(criticalsFile);
        var obfuscation = new BitMonoObfuscationConfiguration(obfuscationFile);
        source.AddOptions()
            .Configure<ProtectionSettings>(protections.Configuration)
            .Configure<Criticals>(criticals.Configuration)
            .Configure<Obfuscation>(obfuscation.Configuration);
        return source;
    }
}