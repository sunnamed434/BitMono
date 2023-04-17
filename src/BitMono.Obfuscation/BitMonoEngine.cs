using BitMono.Core.Services;

#pragma warning disable CS8602
#pragma warning disable CS8604
namespace BitMono.Obfuscation;

[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class BitMonoEngine
{
    private readonly ILifetimeScope _lifetimeScope;
    private readonly ObfuscationAttributeResolver _obfuscationAttributeResolver;
    private readonly ObfuscateAssemblyAttributeResolver _obfuscateAssemblyAttributeResolver;
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly List<ProtectionSetting> _protectionSettings;
    private readonly List<IMemberResolver> _memberResolvers;
    private readonly IEngineContextAccessor _engineContextAccessor;
    private readonly ILogger _logger;

    public BitMonoEngine(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
        _obfuscationAttributeResolver = _lifetimeScope.Resolve<ObfuscationAttributeResolver>();
        _obfuscateAssemblyAttributeResolver = _lifetimeScope.Resolve<ObfuscateAssemblyAttributeResolver>();
        _obfuscationSettings = _lifetimeScope.Resolve<IOptions<ObfuscationSettings>>().Value;
        _protectionSettings = _lifetimeScope.Resolve<IOptions<ProtectionSettings>>().Value.Protections!;
        _memberResolvers = _lifetimeScope
            .Resolve<ICollection<IMemberResolver>>()
            .ToList();
        _engineContextAccessor = _lifetimeScope.Resolve<IEngineContextAccessor>();
        _logger = _lifetimeScope
            .Resolve<ILogger>()
            .ForContext<BitMonoEngine>();
    }

    public async Task<bool> StartAsync(EngineContext engineContext, IDataWriter dataWriter)
    {
        engineContext.ThrowIfCancellationRequested();

        _logger.Information("Loaded Module {0}", engineContext.Module.Name.Value);

        var protections = _lifetimeScope
            .Resolve<ICollection<IProtection>>()
            .ToList();
        var protectionsSorter = new ProtectionsSorter(_obfuscationAttributeResolver, engineContext.Module.Assembly);
        var protectionsSort = protectionsSorter.Sort(protections, _protectionSettings);
        if (protectionsSort.HasProtections == false)
        {
            _logger.Fatal("No one protection were detected, please specify or enable them in protections.json!");
            return false;
        }

        var obfuscator = new BitMonoObfuscator(engineContext, _memberResolvers, protectionsSort, dataWriter,
            _obfuscationAttributeResolver, _obfuscateAssemblyAttributeResolver, _obfuscationSettings, _logger);
        await obfuscator.ProtectAsync();
        return true;
    }
    public async Task<bool> StartAsync(ObfuscationNeeds needs, IModuleFactory moduleFactory, IDataWriter dataWriter,
        IReferencesDataResolver referencesDataResolver, CancellationToken cancellationToken)
    {
        var runtimeModule = ModuleDefinition.FromFile(typeof(Runtime.Data).Assembly.Location);
        var moduleFactoryResult = moduleFactory.Create();
        var bitMonoContextFactory = new BitMonoContextFactory(moduleFactoryResult.Module, referencesDataResolver, _obfuscationSettings);
        var bitMonoContext = bitMonoContextFactory.Create(needs.OutputDirectoryName, needs.FileName);
        var engineContextFactory = new EngineContextFactory(moduleFactoryResult, runtimeModule, bitMonoContext, cancellationToken);
        var engineContext = engineContextFactory.Create();
        _engineContextAccessor.Instance = engineContext;
        bitMonoContext.OutputFile = OutputFilePathFactory.Create(bitMonoContext);
        return await StartAsync(engineContext, dataWriter);
    }
    public async Task<bool> StartAsync(ObfuscationNeeds needs, CancellationToken cancellationToken)
    {
        return await StartAsync(needs,
            new ModuleFactory(File.ReadAllBytes(needs.FileName), new LogErrorListener(_logger)), new FileDataWriter(),
            new AutomaticReferencesDataResolver(needs.ReferencesDirectoryName), cancellationToken);
    }
}