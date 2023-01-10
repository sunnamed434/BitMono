namespace BitMono.Obfuscation;

public class BitMonoEngine
{
    private readonly IDataWriter m_DataWriter;
    private readonly ObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly IBitMonoObfuscationConfiguration m_ObfuscationConfiguration;
    private readonly List<IMemberResolver> m_MemberResolvers;
    private readonly List<IProtection> m_Protections;
    private readonly List<ProtectionSettings> m_ProtectionSettings;
    private readonly ILogger m_Logger;

    public BitMonoEngine(
        IDataWriter dataWriter,
        ObfuscationAttributeResolver obfuscationAttributeResolver,
        IBitMonoObfuscationConfiguration obfuscationConfiguration,
        List<IMemberResolver> memberResolvers,
        List<IProtection> protections,
        List<ProtectionSettings> protectionSettings,
        ILogger logger)
    {
        m_DataWriter = dataWriter;
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_ObfuscationConfiguration = obfuscationConfiguration;
        m_MemberResolvers = memberResolvers;
        m_Protections = protections;
        m_ProtectionSettings = protectionSettings;
        m_Logger = logger.ForContext<BitMonoEngine>();
    }

    internal async Task<bool> StartAsync(ProtectionContext context)
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
        
        var obfuscator = new BitMonoObfuscator(context, m_MemberResolvers, protectionsSort, m_DataWriter, m_ObfuscationAttributeResolver, m_ObfuscationConfiguration, m_Logger);
        await obfuscator.ProtectAsync();
        return true;
    }
    public async Task<bool> StartAsync(BitMonoContext context, IModuleFactory moduleFactory, CancellationToken cancellationToken)
    {
        var runtimeModule = ModuleDefinition.FromFile(typeof(BitMono.Runtime.Data).Assembly.Location);
        var moduleFactoryResult = moduleFactory.Create();
        var protectionContextFactory = new ProtectionContextFactory(moduleFactoryResult, runtimeModule, context, cancellationToken);
        var protectionContext = protectionContextFactory.Create();
        new OutputFilePathFactory().Create(context);
        return await StartAsync(protectionContext);
    }
    public async Task<bool> StartAsync(BitMonoContext context, byte[] data, IErrorListener errorListener, CancellationToken cancellationToken)
    {
        return await StartAsync(context, new ModuleFactory(data, errorListener), cancellationToken);
    }
    public async Task<bool> StartAsync(BitMonoContext context, string fileName, IErrorListener errorListener, CancellationToken cancellationToken)
    {
        return await StartAsync(context, new ModuleFactory(File.ReadAllBytes(fileName), errorListener), cancellationToken);
    }
    public async Task<bool> StartAsync(BitMonoContext context, byte[] data, CancellationToken cancellationToken)
    {
        return await StartAsync(context, data, new LogErrorListener(m_Logger), cancellationToken);
    }
    public async Task<bool> StartAsync(BitMonoContext context, string fileName, CancellationToken cancellationToken)
    {
        return await StartAsync(context, fileName, new LogErrorListener(m_Logger), cancellationToken);
    }
}