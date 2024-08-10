namespace BitMono.Obfuscation;

[SuppressMessage("ReSharper", "InvertIf")]
[SuppressMessage("ReSharper", "ParameterTypeCanBeEnumerable.Local")]
public class BitMonoObfuscator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly StarterContext _context;
    private readonly IDataWriter _dataWriter;
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly InvokablePipeline _invokablePipeline;
    private readonly ObfuscationAttributeResolver _obfuscationAttributeResolver;
    private readonly ObfuscationAttributesStripper _obfuscationAttributesStripper;
    private readonly ObfuscationAttributesStripNotifier _obfuscationAttributesStripNotifier;
    private readonly ProtectionsNotifier _protectionsNotifier;
    private readonly ProtectionExecutionNotifier _protectionExecutionNotifier;
    private readonly ILogger _logger;
    private ProtectionsSort? _protectionsSort;
    private PEImageBuildResult? _imageBuild;
    private long _startTime;

    public BitMonoObfuscator(
        IServiceProvider serviceProvider,
        StarterContext context,
        IDataWriter dataWriter,
        ObfuscationSettings obfuscationSettings,
        ILogger logger)
    {
        _serviceProvider = serviceProvider;
        _context = context;
        _dataWriter = dataWriter;
        _obfuscationSettings = obfuscationSettings;
        _invokablePipeline = new InvokablePipeline();
        _obfuscationAttributeResolver = _serviceProvider.GetRequiredService<ObfuscationAttributeResolver>();
        var obfuscateAssemblyAttributeResolver = _serviceProvider.GetRequiredService<ObfuscateAssemblyAttributeResolver>();
        _obfuscationAttributesStripper = new ObfuscationAttributesStripper(_obfuscationSettings,
            _obfuscationAttributeResolver, obfuscateAssemblyAttributeResolver);
        _logger = logger.ForContext<BitMonoObfuscator>();
        _obfuscationAttributesStripNotifier = new ObfuscationAttributesStripNotifier(_logger);
        _protectionsNotifier = new ProtectionsNotifier(_obfuscationSettings, _logger);
        _protectionExecutionNotifier = new ProtectionExecutionNotifier(_logger);
    }

    public async Task ProtectAsync()
    {
        _context.ThrowIfCancellationRequested();

        _invokablePipeline.OnFail += OnFailHandleAsync;

        await _invokablePipeline.InvokeAsync(OutputLoadedModuleAsync);
        await _invokablePipeline.InvokeAsync(SortProtectionsAsync);
        await _invokablePipeline.InvokeAsync(OutputProtectionsAsync);
        await _invokablePipeline.InvokeAsync(StartTimeCounterAsync);
        await _invokablePipeline.InvokeAsync(OutputFrameworkInformationAsync);
        await _invokablePipeline.InvokeAsync(ResolveDependenciesAsync);
        await _invokablePipeline.InvokeAsync(ExpandMacrosAsync);
        await _invokablePipeline.InvokeAsync(RunProtectionsAsync);
        await _invokablePipeline.InvokeAsync(OptimizeMacrosAsync);
        await _invokablePipeline.InvokeAsync(StripObfuscationAttributesAsync);
        await _invokablePipeline.InvokeAsync(CreatePEImageAsync);
        await _invokablePipeline.InvokeAsync(WriteModuleAsync);
        await _invokablePipeline.InvokeAsync(PackAsync);
        await _invokablePipeline.InvokeAsync(OutputElapsedTimeAsync);
    }

    private Task<bool> OutputLoadedModuleAsync()
    {
        var targetFrameworkName = "unknown";
        if (_context.Module.Assembly!.TryGetTargetFramework(out var info))
        {
            targetFrameworkName = info.Name;
        }

        var assemblyInfo = _context.Module.Assembly.ToString();
        var culture = _context.Module.Assembly.Culture ?? "unknown";
        var timeDateStamp = _context.Module.ToPEImage().TimeDateStamp;
        _logger.Information("Module {0}", assemblyInfo);
        _logger.Information("Module Target Framework: {0}", targetFrameworkName);
        _logger.Information("PE TimeDateStamp: {0}", timeDateStamp);
        _logger.Information("Module culture: {0}", culture);
        return Task.FromResult(true);
    }
    private Task<bool> SortProtectionsAsync()
    {
        var protectionSettings = _serviceProvider.GetRequiredService<IOptions<ProtectionSettings>>().Value.Protections!;
        var protections = _serviceProvider
            .GetRequiredService<ICollection<IProtection>>()
            .ToList();
        var protectionsSorter = new ProtectionsSorter(_obfuscationAttributeResolver, _context.Module.Assembly!);
        _protectionsSort = protectionsSorter.Sort(protections, protectionSettings);
        if (_protectionsSort.HasProtections == false)
        {
            _logger.Fatal("No one protection were detected, please specify or enable them in protections.json!");
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }
    private Task<bool> OutputProtectionsAsync()
    {
        if (_protectionsSort == null)
        {
            _logger.Fatal("Unable to output protections without sorted protections!");
            return Task.FromResult(false);
        }
        _protectionsNotifier.Notify(_protectionsSort);
        return Task.FromResult(true);
    }
    private Task<bool> StartTimeCounterAsync()
    {
        _startTime = Stopwatch.GetTimestamp();
        return Task.FromResult(true);
    }
    private Task<bool> OutputFrameworkInformationAsync()
    {
        _logger.Information(RuntimeUtilities.GetFrameworkInformation().ToString());
        return Task.FromResult(true);
    }
    private Task<bool> ResolveDependenciesAsync()
    {
        _logger.Information("Starting resolving dependencies...");
        var assemblyResolve = AssemblyResolver.Resolve(_context.BitMonoContext.ReferencesData, _context);
        foreach (var reference in assemblyResolve.ResolvedReferences)
        {
            _context.ThrowIfCancellationRequested();

            _logger.Information("Successfully resolved dependency: {0}", reference.FullName);
        }
        foreach (var reference in assemblyResolve.FailedToResolveReferences)
        {
            _context.ThrowIfCancellationRequested();

            _logger.Warning("Failed to resolve dependency: {0}", reference.FullName);
        }
        _logger.Information("References resolve have been completed!");
        if (assemblyResolve.Succeed == false)
        {
            if (_obfuscationSettings.FailOnNoRequiredDependency)
            {
                _logger.Fatal("Please, specify needed dependencies, or set in {0} FailOnNoRequiredDependency to false",
                    "obfuscation.json");
                _logger.Warning(
                    "Unresolved dependencies aren't a major issue, but keep in mind they can cause problems or might result in some parts being missed during obfuscation.");
                return Task.FromResult(false);
            }
        }
        return Task.FromResult(true);
    }
    private Task<bool> ExpandMacrosAsync()
    {
        foreach (var method in _context.Module.FindMembers().OfType<MethodDefinition>())
        {
            _context.ThrowIfCancellationRequested();

            if (method.CilMethodBody is not { } body)
            {
                continue;
            }

            body.Instructions.ExpandMacros();
        }
        return Task.FromResult(true);
    }
    private async Task<bool> RunProtectionsAsync()
    {
        _logger.Information("Executing Protections... this could take for a while...");
        foreach (var protection in _protectionsSort!.SortedProtections)
        {
            _context.ThrowIfCancellationRequested();

            await protection.ExecuteAsync();
            _protectionExecutionNotifier.Notify(protection);
        }
        foreach (var pipeline in _protectionsSort.Pipelines)
        {
            _context.ThrowIfCancellationRequested();

            await pipeline.ExecuteAsync();
            _protectionExecutionNotifier.Notify(pipeline);

            foreach (var phase in pipeline.PopulatePipeline())
            {
                _context.ThrowIfCancellationRequested();

                await phase.ExecuteAsync();
                _protectionExecutionNotifier.Notify(phase);
            }
        }
        return true;
    }
    private Task<bool> OptimizeMacrosAsync()
    {
        foreach (var method in _context.Module.FindMembers().OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is { } body)
            {
                body.Instructions.OptimizeMacros();
            }
        }
        return Task.FromResult(true);
    }
    private Task<bool> StripObfuscationAttributesAsync()
    {
        var obfuscationAttributesStrip = _obfuscationAttributesStripper.Strip(_context, _protectionsSort!);
        _obfuscationAttributesStripNotifier.Notify(obfuscationAttributesStrip);
        return Task.FromResult(true);
    }
    private Task<bool> CreatePEImageAsync()
    {
        _imageBuild = _context.PEImageBuilder.CreateImage(_context.Module);
        if (_imageBuild == null || _imageBuild.HasFailed)
        {
            _logger.Fatal("Unable to construct the PE image!");
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }
    private async Task<bool> WriteModuleAsync()
    {
        try
        {
            using var memoryStream = new MemoryStream();
            var fileBuilder = new ManagedPEFileBuilder();
            fileBuilder
                .CreateFile(_imageBuild!.ConstructedImage!)
                .Write(memoryStream);
            await _dataWriter.WriteAsync(_context.BitMonoContext.OutputFile, memoryStream.ToArray());
            _logger.Information("The protected module was saved in {0}", _context.BitMonoContext.OutputDirectoryName);
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "An error occured while writing the module!");
            return false;
        }
        return true;
    }
    private async Task<bool> PackAsync()
    {
        foreach (var packer in _protectionsSort!.Packers)
        {
            _context.ThrowIfCancellationRequested();

            await packer.ExecuteAsync();
            _protectionExecutionNotifier.Notify(packer);
        }
        _logger.Information("Protections have been executed!");
        return true;
    }
    private Task<bool> OutputElapsedTimeAsync()
    {
        var elapsedTime = StopwatchUtilities.GetElapsedTime(_startTime, Stopwatch.GetTimestamp());
        _logger.Information("Since obfuscation elapsed: {0}", elapsedTime.ToString());
        return Task.FromResult(true);
    }
    private Task OnFailHandleAsync()
    {
        _logger.Fatal("Obfuscation stopped! Something went wrong!");
        return Task.CompletedTask;
    }
}