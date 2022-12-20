namespace BitMono.Obfuscation;

public class BitMonoEngine
{
    private readonly IDataWriter m_DataWriter;
    private readonly IObfuscationAttributeResolver m_DnlibDefObfuscationAttributeResolver;
    private readonly IBitMonoObfuscationConfiguration m_ObfuscationConfiguratin;
    private readonly List<IMemberDefinitionfResolver> m_DnlibDefResolvers;
    private readonly List<IProtection> m_Protections;
    private readonly List<ProtectionSettings> m_ProtectionSettings;
    private readonly ILogger m_Logger;

    public BitMonoEngine(
        IDataWriter dataWriter,
        IObfuscationAttributeResolver dnlibDefObfuscationAttributeResolver,
        IBitMonoObfuscationConfiguration obfuscationConfiguration,
        List<IMemberDefinitionfResolver> dnlibDefResolvers,
        List<IProtection> protections,
        List<ProtectionSettings> protectionSettings,
        ILogger logger)
    {
        m_DataWriter = dataWriter;
        m_DnlibDefObfuscationAttributeResolver = dnlibDefObfuscationAttributeResolver;
        m_ObfuscationConfiguratin = obfuscationConfiguration;
        m_DnlibDefResolvers = dnlibDefResolvers;
        m_Protections = protections;
        m_ProtectionSettings = protectionSettings;
        m_Logger = logger.ForContext<BitMonoEngine>();
    }

    public async Task ObfuscateAsync(BitMonoContext context, IModuleCreator moduleCreator, CancellationTokenSource cancellationTokenSource)
    {
        cancellationTokenSource.Token.ThrowIfCancellationRequested();

        var moduleDefMDCreationResult = moduleCreator.Create();
        var runtimeModuleDefinition = ModuleDefinition.FromModule(typeof(BitMono.Runtime.Data).Module);
        var protectionContext = new ProtectionContextCreator(moduleDefMDCreationResult, runtimeModuleDefinition, context).Create();
        new OutputFilePathCreator().Create(context);
        m_Logger.Information("Loaded Module {0}", protectionContext.Module.Name);

        var protectionsSortResult = new ProtectionsSorter(m_DnlibDefObfuscationAttributeResolver, protectionContext.Module.Assembly, m_Logger)
            .Sort(m_Protections, m_ProtectionSettings);

        if (protectionsSortResult.HasProtections == false)
        {
            m_Logger.Fatal("No one protection were detected!");
            cancellationTokenSource.Cancel();
            return;
        }
        if (m_ObfuscationConfiguratin.Configuration.GetValue<bool>(nameof(Shared.Models.Obfuscation.FailOnNoRequiredDependency)))
        {
            var resolvingSucceed = new BitMonoAssemblyResolver(m_Logger).Resolve(protectionContext.BitMonoContext.DependenciesData, protectionContext, cancellationTokenSource.Token);
            if (resolvingSucceed == false)
            {
                m_Logger.Fatal("Drop dependencies in 'libs' directory with the same path as your module has, or set in obfuscation.json FailOnNoRequiredDependency to false");
                cancellationTokenSource.Cancel();
                return;
            }
        }
        
        new ProtectionsExecutionNotifier(m_Logger).Notify(protectionsSortResult);
        
        m_Logger.Information("Preparing to protect module: {0}", context.FileName);
        await new BitMonoObfuscator(
            m_DnlibDefResolvers,
            protectionsSortResult.Protections,
            protectionsSortResult.Packers,
            protectionContext,
            m_DataWriter,
            m_Logger)
            .StartAsync(cancellationTokenSource);
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