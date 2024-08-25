namespace BitMono.Obfuscation;

public class BitMonoObfuscator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly StarterContext _context;
    private readonly IDataWriter _dataWriter;
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly InvokablePipeline _pipeline;
    private readonly ObfuscationAttributeResolver _obfuscationAttributeResolver;
    private readonly ObfuscationAttributesStripper _obfuscationAttributesStripper;
    private readonly ObfuscationAttributesStripNotifier _obfuscationAttributesStripNotifier;
    private readonly ProtectionsNotifier _protectionsNotifier;
    private readonly ProtectionsConfigureForNativeCodeNotifier _protectionsConfigureForNativeCodeNotifier;
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
        _pipeline = new InvokablePipeline();
        _obfuscationAttributeResolver = _serviceProvider.GetRequiredService<ObfuscationAttributeResolver>();
        var obfuscateAssemblyAttributeResolver = _serviceProvider.GetRequiredService<ObfuscateAssemblyAttributeResolver>();
        _obfuscationAttributesStripper = new ObfuscationAttributesStripper(
            _obfuscationAttributeResolver, obfuscateAssemblyAttributeResolver);
        _logger = logger.ForContext<BitMonoObfuscator>();
        _obfuscationAttributesStripNotifier = new ObfuscationAttributesStripNotifier(_logger);
        _protectionsNotifier = new ProtectionsNotifier(_obfuscationSettings, _logger);
        _protectionsConfigureForNativeCodeNotifier = new ProtectionsConfigureForNativeCodeNotifier(_obfuscationSettings, _logger);
        _protectionExecutionNotifier = new ProtectionExecutionNotifier(_logger);
    }

    public async Task ProtectAsync()
    {
        _context.ThrowIfCancellationRequested();

        _pipeline.OnFail += OnFailHandleAsync;

        await _pipeline.InvokeAsync(OutputLoadedModule);
        await _pipeline.InvokeAsync(OutputBitMonoInfo);
        await _pipeline.InvokeAsync(OutputCompatibilityIssues);
        await _pipeline.InvokeAsync(SortProtections);
        await _pipeline.InvokeAsync(OutputProtectionsAsync);
        await _pipeline.InvokeAsync(ConfigureForNativeCode);
        await _pipeline.InvokeAsync(StartTimeCounter);
        await _pipeline.InvokeAsync(ResolveDependencies);
        await _pipeline.InvokeAsync(ExpandMacros);
        await _pipeline.InvokeAsync(RunProtectionsAsync);
        await _pipeline.InvokeAsync(OptimizeMacros);
        await _pipeline.InvokeAsync(StripObfuscationAttributes);
        await _pipeline.InvokeAsync(CreatePEImage);
        await _pipeline.InvokeAsync(WriteModuleAsync);
        await _pipeline.InvokeAsync(PackAsync);
        await _pipeline.InvokeAsync(OutputElapsedTime);
    }

    private void OutputLoadedModule()
    {
        var targetFrameworkText = "unknown";
        var module = _context.Module;
        var assembly = module.Assembly;
        if (assembly!.TryGetTargetFramework(out var info))
        {
            targetFrameworkText = $"{info.Name} {info.Version}";
        }

        var assemblyInfo = assembly.ToString();
        var culture = assembly.Culture?.ToString() ?? "unknown";
        var timeDateStamp = module.ToPEImage().TimeDateStamp;
        _logger.Information("Module {0}", assemblyInfo);
        _logger.Information("Module Target Framework: {0}", targetFrameworkText);
        _logger.Information("Module PE TimeDateStamp: {0}", timeDateStamp);
        _logger.Information("Module culture: {0}", culture);
    }
    private void OutputBitMonoInfo()
    {
        _logger.Information(EnvironmentRuntimeInformation.Create().ToString());
    }
    /// <summary>
    /// Outputs information in case of module is built for .NET Framework,
    /// but BitMono is running on .NET Core, or vice versa.
    /// See more info: https://bitmono.readthedocs.io/en/latest/obfuscationissues/corlib-not-found.html
    /// </summary>
    private void OutputCompatibilityIssues()
    {
        if (_context.Module.Assembly!.TryGetTargetFramework(out var targetAssemblyRuntime) == false)
        {
            return;
        }
        if (targetAssemblyRuntime.IsNetCoreApp && DotNetRuntimeInfoEx.IsNetFramework())
        {
            _logger.Warning(
                "The module is built for .NET (Core), but you're using a version of BitMono intended for .NET Framework." +
                " To avoid potential issues, ensure the target framework matches the BitMono framework, " +
                "or switch to a .NET Core build of BitMono.");
            return;
        }
        if (targetAssemblyRuntime.IsNetFramework && DotNetRuntimeInfoEx.IsNetCoreOrLater())
        {
            _logger.Warning(
                "The module is built for .NET Framework, but you're using a version of BitMono intended for .NET (Core)." +
                " To avoid potential issues, ensure the target framework matches the BitMono framework, " +
                "or switch to a .NET Framework build of BitMono.");
            return;
        }
    }
    private bool SortProtections()
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
            return false;
        }
        return true;
    }
    private bool OutputProtectionsAsync()
    {
        if (_protectionsSort == null)
        {
            _logger.Fatal("Unable to output protections without sorted protections!");
            return false;
        }
        _protectionsNotifier.Notify(_protectionsSort, _context.CancellationToken);
        return true;
    }
    private void ConfigureForNativeCode()
    {
        if (_protectionsSort!.ConfigureForNativeCodeProtections.Any() == false)
        {
            return;
        }

        _protectionsConfigureForNativeCodeNotifier.Notify(_protectionsSort, _context.CancellationToken);

        var module = _context.Module;
        module.IsILOnly = false;
        var x64 = module.MachineType == MachineType.Amd64;
        if (x64)
        {
            module.PEKind = OptionalHeaderMagic.PE32Plus;
            module.MachineType = MachineType.Amd64;
            module.IsBit32Required = false;
        }
        else
        {
            module.PEKind = OptionalHeaderMagic.PE32;
            module.MachineType = MachineType.I386;
            module.IsBit32Required = true;
        }
    }
    private void StartTimeCounter()
    {
        _startTime = Stopwatch.GetTimestamp();
    }
    private bool ResolveDependencies()
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
                _logger.Fatal("Please, specify needed dependencies, or set in {0} {1} to false",
                    "obfuscation.json", nameof(ObfuscationSettings.FailOnNoRequiredDependency));
                _logger.Warning(
                    "Unresolved dependencies aren't a major issue, but keep in mind they can cause problems or might result in some parts being missed during obfuscation.");
                return false;
            }
        }
        return true;
    }
    private void ExpandMacros()
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
    }
    private async Task RunProtectionsAsync()
    {
        _logger.Information("Executing Protections... this could take for a while...");
        if (_protectionsSort == null)
        {
            throw new InvalidOperationException($"{nameof(_protectionsSort)} was null!");
        }
        foreach (var protection in _protectionsSort.SortedProtections)
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
    }
    private void OptimizeMacros()
    {
        foreach (var method in _context.Module.FindMembers().OfType<MethodDefinition>())
        {
            _context.ThrowIfCancellationRequested();

            if (method.CilMethodBody is not { } body)
            {
                continue;
            }

            body.Instructions.OptimizeMacros();
        }
    }
    private void StripObfuscationAttributes()
    {
        if (_obfuscationSettings.StripObfuscationAttributes == false)
        {
            return;
        }
        var obfuscationAttributesStrip = _obfuscationAttributesStripper.Strip(_context, _protectionsSort!);
        _obfuscationAttributesStripNotifier.Notify(obfuscationAttributesStrip);
    }
    private bool CreatePEImage()
    {
        _imageBuild = _context.PEImageBuilder.CreateImage(_context.Module);
        if (_imageBuild == null || _imageBuild.HasFailed)
        {
            _logger.Fatal("Unable to construct the PE image!");
            return false;
        }
        return true;
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
    private async Task PackAsync()
    {
        foreach (var packer in _protectionsSort!.Packers)
        {
            _context.ThrowIfCancellationRequested();

            await packer.ExecuteAsync();
            _protectionExecutionNotifier.Notify(packer);
        }
        _logger.Information("Protections have been executed!");
    }
    private void OutputElapsedTime()
    {
        var elapsedTime = StopwatchUtilities.GetElapsedTime(_startTime, Stopwatch.GetTimestamp());
        _logger.Information("Since obfuscation elapsed: {0}", elapsedTime.ToString());
    }
    private Task OnFailHandleAsync()
    {
        _logger.Fatal("Obfuscation stopped! Something went wrong!");
        return Task.CompletedTask;
    }
}