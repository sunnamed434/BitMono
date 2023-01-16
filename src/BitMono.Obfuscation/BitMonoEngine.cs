namespace BitMono.Obfuscation;

public class BitMonoEngine
{
    private readonly ObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly Shared.Models.Obfuscation m_Obfuscation;
    private readonly List<ProtectionSetting> m_ProtectionSettings;
    private readonly List<IMemberResolver> m_MemberResolvers;
    private readonly List<IProtection> m_Protections;
    private readonly ILogger m_Logger;

    public BitMonoEngine(
        ObfuscationAttributeResolver obfuscationAttributeResolver,
        Shared.Models.Obfuscation obfuscation,
        ProtectionSettings protectionSettings,
        List<IMemberResolver> memberResolvers,
        List<IProtection> protections,
        ILogger logger)
    {
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_Obfuscation = obfuscation;
        m_MemberResolvers = memberResolvers;
        m_Protections = protections;
        m_ProtectionSettings = protectionSettings.Protections;
        m_Logger = logger.ForContext<BitMonoEngine>();
    }

    internal async Task<bool> StartAsync(ProtectionContext context, IDataWriter dataWriter)
    {
        context.ThrowIfCancellationRequested();
        
        m_Logger.Information("Loaded Module {0}", context.Module.Name.Value);

        var protectionsSorter = new ProtectionsSorter(m_ObfuscationAttributeResolver, context.Module.Assembly);
        var protectionsSort = protectionsSorter.Sort(m_Protections, m_ProtectionSettings);
        if (protectionsSort.HasProtections == false)
        {
            m_Logger.Fatal("No one protection were detected!");
            return false;
        }
        
        var obfuscator = new BitMonoObfuscator(context, m_MemberResolvers, protectionsSort, dataWriter, m_ObfuscationAttributeResolver, m_Obfuscation, m_Logger);
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
        new OutputFilePathFactory().Create(bitMonoContext);
        return await StartAsync(protectionContext, dataWriter);
    }
    public async Task<bool> StartAsync(ObfuscationNeeds needs, CancellationToken cancellationToken)
    {
        return await StartAsync(needs, new ModuleFactory(File.ReadAllBytes(needs.FileName), new LogErrorListener(m_Logger)), new FileDataWriter(), cancellationToken);
    }
}