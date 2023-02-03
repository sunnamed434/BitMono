namespace BitMono.Host.Extensions;

public static class AutofacServiceProviderExtensions
{
    private static readonly string ProtectionsFileName = $"{typeof(BitMono.Protections.BitMono).Namespace}.dll";
    public static ContainerBuilder AddProtections(this ContainerBuilder source)
    {
        Assembly.Load(File.ReadAllBytes(ProtectionsFileName));

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        source.RegisterAssemblyTypes(assemblies)
            .PublicOnly()
            .Where(t => t.GetInterface(nameof(IPhaseProtection)) == null && t.GetInterface(nameof(IProtection)) != null)
            .OwnedByLifetimeScope()
            .AsImplementedInterfaces()
            .SingleInstance();
        return source;
    }
    public static ServiceCollection AddConfigurations(this ServiceCollection source)
    {
        var protections = new BitMonoProtectionsConfiguration();
        var criticals = new BitMonoCriticalsConfiguration();
        var obfuscation = new BitMonoObfuscationConfiguration();
        source.AddOptions();
        source.Configure<ProtectionSettings>(options => protections.Configuration.Bind(options));
        source.Configure<Criticals>(criticals.Configuration);
        source.Configure<Obfuscation>(obfuscation.Configuration);
        return source;
    }
}