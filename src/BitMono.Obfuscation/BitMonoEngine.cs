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
        
        new ProtectionsNotifier(m_ObfuscationConfiguration, m_Logger).Notify(protectionsSort);
        
        m_Logger.Information("Preparing to protect module: {0}", context.BitMonoContext.FileName);
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        await new BitMonoObfuscator(
            context,
            m_MemberResolvers,
            protectionsSort,
            m_DataWriter,
            m_ObfuscationAttributeResolver,
            m_ObfuscationConfiguration,
            m_Logger)
            .ProtectAsync();
        stopWatch.Stop();
        m_Logger.Information("Protected module`s saved in {0}", context.BitMonoContext.OutputDirectoryName);
        m_Logger.Information("Elapsed: {0}", stopWatch.Elapsed.ToString());
        return true;
    }
    public async Task<bool> StartAsync(BitMonoContext context, IModuleCreator moduleCreator, CancellationToken cancellationToken)
    {
        var moduleDefMDCreationResult = moduleCreator.Create();
        var runtimeModule = ModuleDefinition.FromFile(typeof(BitMono.Runtime.Data).Assembly.Location);
        var protectionContext = new ProtectionContextCreator
        {
            ModuleCreationResult = moduleDefMDCreationResult,
            RuntimeModuleDefinition = runtimeModule,
            BitMonoContext = context,
            CancellationToken = cancellationToken
        }.Create();
        new OutputFilePathCreator().Create(context);
        return await StartAsync(protectionContext);
    }
    public async Task<bool> StartAsync(BitMonoContext context, byte[] data, IErrorListener errorListener, CancellationToken cancellationToken)
    {
        return await StartAsync(context, new ModuleCreator(data, errorListener), cancellationToken);
    }
    public async Task<bool> StartAsync(BitMonoContext context, string fileName, IErrorListener errorListener, CancellationToken cancellationToken)
    {
        return await StartAsync(context, new ModuleCreator(File.ReadAllBytes(fileName), errorListener), cancellationToken);
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