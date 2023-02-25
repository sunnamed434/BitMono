#pragma warning disable CS8602
#pragma warning disable CS8604
namespace BitMono.Obfuscation;

[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class BitMonoEngine
{
    private readonly ILifetimeScope m_LifetimeScope;
    private readonly ObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly ObfuscateAssemblyAttributeResolver m_ObfuscateAssemblyAttributeResolver;
    private readonly Shared.Models.Obfuscation m_Obfuscation;
    private readonly List<ProtectionSetting> m_ProtectionSettings;
    private readonly List<IMemberResolver> m_MemberResolvers;
    private readonly ILogger m_Logger;

    public BitMonoEngine(ILifetimeScope lifetimeScope)
    {
        m_LifetimeScope = lifetimeScope;
        m_ObfuscationAttributeResolver = m_LifetimeScope.Resolve<ObfuscationAttributeResolver>();
        m_ObfuscateAssemblyAttributeResolver = m_LifetimeScope.Resolve<ObfuscateAssemblyAttributeResolver>();
        m_Obfuscation = m_LifetimeScope.Resolve<IOptions<Shared.Models.Obfuscation>>().Value;
        m_ProtectionSettings = m_LifetimeScope.Resolve<IOptions<ProtectionSettings>>().Value.Protections;
        m_MemberResolvers = m_LifetimeScope
            .Resolve<ICollection<IMemberResolver>>()
            .ToList();;
        m_Logger = m_LifetimeScope
            .Resolve<ILogger>()
            .ForContext<BitMonoEngine>();
    }

    internal async Task<bool> StartAsync(ProtectionContext context, IDataWriter dataWriter)
    {
        context.ThrowIfCancellationRequested();

        m_Logger.Information("Loaded Module {0}", context.Module.Name.Value);

        var protections = m_LifetimeScope
            .Resolve<ICollection<IProtection>>(new TypedParameter(typeof(ProtectionContext), context))
            .ToList();
        Console.WriteLine("Protections count: " + protections.Count);
        var protectionsSorter = new ProtectionsSorter(m_ObfuscationAttributeResolver, context.Module.Assembly);
        var protectionsSort = protectionsSorter.Sort(protections, m_ProtectionSettings);
        if (protectionsSort.HasProtections == false)
        {
            m_Logger.Fatal("No one protection were detected, please specify or enable them in protections.json!");
            return false;
        }

        var obfuscator = new BitMonoObfuscator(context, m_MemberResolvers, protectionsSort, dataWriter, m_ObfuscationAttributeResolver, m_ObfuscateAssemblyAttributeResolver, m_Obfuscation, m_Logger);
        await obfuscator.ProtectAsync();
        return true;
    }
    public async Task<bool> StartAsync(ObfuscationNeeds needs, IModuleFactory moduleFactory, IDataWriter dataWriter, CancellationToken cancellationToken)
    {
        var dependenciesDataResolver = new DependenciesDataResolver(needs.DependenciesDirectoryName);
        var bitMonoContextFactory = new BitMonoContextFactory(dependenciesDataResolver, m_Obfuscation);
        var bitMonoContext = bitMonoContextFactory.Create(needs.OutputDirectoryName, needs.FileName);

        var runtimeModule = ModuleDefinition.FromFile(typeof(BitMono.Runtime.Data).Assembly.Location);
        var moduleFactoryResult = moduleFactory.Create();
        var protectionContextFactory = new ProtectionContextFactory(moduleFactoryResult, runtimeModule, bitMonoContext, cancellationToken);
        var protectionContext = protectionContextFactory.Create();
        bitMonoContext.OutputFile = OutputFilePathFactory.Create(bitMonoContext);
        return await StartAsync(protectionContext, dataWriter);
    }
    public async Task<bool> StartAsync(ObfuscationNeeds needs, CancellationToken cancellationToken)
    {
        return await StartAsync(needs, new ModuleFactory(File.ReadAllBytes(needs.FileName), new LogErrorListener(m_Logger)), new FileDataWriter(), cancellationToken);
    }
}