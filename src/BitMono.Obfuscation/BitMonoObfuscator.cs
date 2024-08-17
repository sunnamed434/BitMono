﻿namespace BitMono.Obfuscation;

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
        _obfuscationAttributesStripper = new ObfuscationAttributesStripper(
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

        await _invokablePipeline.InvokeAsync(OutputLoadedModule);
        await _invokablePipeline.InvokeAsync(OutputBitMonoInfo);
        await _invokablePipeline.InvokeAsync(OutputCompatibilityIssues);
        await _invokablePipeline.InvokeAsync(SortProtections);
        await _invokablePipeline.InvokeAsync(OutputProtectionsAsync);
        await _invokablePipeline.InvokeAsync(StartTimeCounter);
        await _invokablePipeline.InvokeAsync(ResolveDependencies);
        await _invokablePipeline.InvokeAsync(ExpandMacros);
        await _invokablePipeline.InvokeAsync(RunProtectionsAsync);
        await _invokablePipeline.InvokeAsync(OptimizeMacros);
        await _invokablePipeline.InvokeAsync(StripObfuscationAttributes);
        await _invokablePipeline.InvokeAsync(CreatePEImage);
        await _invokablePipeline.InvokeAsync(WriteModuleAsync);
        await _invokablePipeline.InvokeAsync(PackAsync);
        await _invokablePipeline.InvokeAsync(OutputElapsedTime);
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
    private bool OutputCompatibilityIssues()
    {
        if (_context.Module.Assembly!.TryGetTargetFramework(out var targetAssemblyRuntime) == false)
        {
            return true;
        }
        if (targetAssemblyRuntime.IsNetCoreApp && DotNetRuntimeInfoEx.IsNetFramework())
        {
            _logger.Warning(
                "The module is built for .NET (Core), but you're using a version of BitMono intended for .NET Framework." +
                " To avoid potential issues, ensure the target framework matches the BitMono framework, " +
                "or switch to a .NET Core build of BitMono.");
            return true;
        }
        if (targetAssemblyRuntime.IsNetFramework && DotNetRuntimeInfoEx.IsNetCoreOrLater())
        {
            _logger.Warning(
                "The module is built for .NET Framework, but you're using a version of BitMono intended for .NET (Core)." +
                " To avoid potential issues, ensure the target framework matches the BitMono framework, " +
                "or switch to a .NET Framework build of BitMono.");
        }
        return true;
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
    private bool ExpandMacros()
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
        return true;
    }
    private async Task<bool> RunProtectionsAsync()
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
        return true;
    }
    private bool OptimizeMacros()
    {
        foreach (var method in _context.Module.FindMembers().OfType<MethodDefinition>())
        {
            _context.ThrowIfCancellationRequested();

            if (method.CilMethodBody is not { } body)
            {
                return true;
            }

            body.Instructions.OptimizeMacros();
        }
        return true;
    }
    private bool StripObfuscationAttributes()
    {
        if (_obfuscationSettings.StripObfuscationAttributes == false)
        {
            _logger.Information("Obfuscation attributes stripping is disabled (it's ok)");
            return true;
        }
        var obfuscationAttributesStrip = _obfuscationAttributesStripper.Strip(_context, _protectionsSort!);
        _obfuscationAttributesStripNotifier.Notify(obfuscationAttributesStrip);
        return true;
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