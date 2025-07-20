namespace BitMono.Host.Extensions;

[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public static class AutofacContainerBuilderExtensions
{
    private const string ProtectionsFileName = "BitMono.Protections.dll";

    public static ContainerBuilder AddProtections(this ContainerBuilder source, string? file = null)
    {
        var protectionsFilePath = file ?? Path.Combine(AppContext.BaseDirectory, ProtectionsFileName);
        var rawData = File.ReadAllBytes(file ?? protectionsFilePath);
        Assembly.Load(rawData);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        source.RegisterAssemblyTypes(assemblies)
            .PublicOnly()
            .Where(x => x.GetInterface(nameof(IPhaseProtection)) == null && x.GetInterface(nameof(IProtection)) != null)
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
            .Configure<CriticalsSettings>(criticals.Configuration)
            .Configure<ObfuscationSettings>(obfuscation.Configuration);
        return source;
    }
}