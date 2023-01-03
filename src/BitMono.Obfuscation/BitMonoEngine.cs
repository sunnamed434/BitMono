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

    public async Task ObfuscateAsync(BitMonoContext context, IModuleCreator moduleCreator, CancellationTokenSource cancellationTokenSource)
    {
        var cancellationToken = cancellationTokenSource.Token;
        cancellationToken.ThrowIfCancellationRequested();

        var moduleDefMDCreationResult = moduleCreator.Create();
        var runtimeModuleDefinition = ModuleDefinition.FromFile(typeof(BitMono.Runtime.Data).Assembly.Location);
        var protectionContext = new ProtectionContextCreator
        {
            ModuleCreationResult = moduleDefMDCreationResult, 
            RuntimeModuleDefinition = runtimeModuleDefinition,
            BitMonoContext = context,
            CancellationToken = cancellationToken
        }.Create();
        new OutputFilePathCreator().Create(context);
        m_Logger.Information("Loaded Module {0}", protectionContext.Module.Name.Value);

        var protectionsSort = new ProtectionsSorter(m_ObfuscationAttributeResolver, protectionContext.Module.Assembly)
            .Sort(m_Protections, m_ProtectionSettings);

        if (protectionsSort.HasProtections == false)
        {
            m_Logger.Fatal("No one protection were detected!");
            cancellationTokenSource.Cancel();
            return;
        }
        
        new ProtectionsNotifier(m_ObfuscationConfiguration, m_Logger).Notify(protectionsSort);
        
        m_Logger.Information("Preparing to protect module: {0}", context.FileName);
        await new BitMonoObfuscator(
            protectionContext,
            m_MemberResolvers,
            protectionsSort,
            m_DataWriter,
            m_ObfuscationAttributeResolver,
            m_ObfuscationConfiguration,
            m_Logger)
            .StartAsync();
        m_Logger.Information("Protected module`s saved in {0}", context.OutputDirectoryName);
    }
    public async Task ObfuscateAsync(BitMonoContext context, byte[] data, CancellationTokenSource cancellationTokenSource)
    {
        await ObfuscateAsync(context, new ModuleCreator(data), cancellationTokenSource);
    }
    public async Task ObfuscateAsync(BitMonoContext context, string fileName, CancellationTokenSource cancellationTokenSource)
    {
        await ObfuscateAsync(context, new ModuleCreator(File.ReadAllBytes(fileName)), cancellationTokenSource);
    }
}