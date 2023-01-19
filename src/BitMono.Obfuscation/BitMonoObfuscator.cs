namespace BitMono.Obfuscation;

public class BitMonoObfuscator
{
    private readonly ProtectionContext m_Context;
    private readonly IEnumerable<IMemberResolver> m_MemberResolvers;
    private readonly ProtectionsSort m_ProtectionsSort;
    private readonly IDataWriter m_DataWriter;
    private readonly ObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly ObfuscateAssemblyAttributeResolver m_ObfuscateAssemblyAttributeResolver;
    private readonly Shared.Models.Obfuscation m_Obfuscation;
    private readonly IInvokablePipeline m_InvokablePipeline;
    private readonly MembersResolver m_MemberResolver;
    private readonly ProtectionExecutionNotifier m_ProtectionExecutionNotifier;
    private readonly ProtectionsNotifier m_ProtectionsNotifier;
    private readonly ObfuscationAttributesStripper m_ObfuscationAttributesStripper;
    private readonly ObfuscationAttributesStripNotifier m_ObfuscationAttributesStripNotifier;
    private readonly ILogger m_Logger;
    private PEImageBuildResult _imageBuild;
    private long _startTime;

    public BitMonoObfuscator(
        ProtectionContext context,
        IEnumerable<IMemberResolver> memberResolvers,
        ProtectionsSort protectionsSortResult,
        IDataWriter dataWriter,
        ObfuscationAttributeResolver obfuscationAttributeResolver,
        ObfuscateAssemblyAttributeResolver obfuscateAssemblyAttributeResolver,
        Shared.Models.Obfuscation obfuscation,
        ILogger logger)
    {
        m_Context = context;
        m_MemberResolvers = memberResolvers;
        m_ProtectionsSort = protectionsSortResult;
        m_DataWriter = dataWriter;
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_ObfuscateAssemblyAttributeResolver = obfuscateAssemblyAttributeResolver;
        m_Obfuscation = obfuscation;
        m_Logger = logger.ForContext<BitMonoObfuscator>();
        m_InvokablePipeline = new InvokablePipeline(m_Context);
        m_MemberResolver = new MembersResolver();
        m_ProtectionExecutionNotifier = new ProtectionExecutionNotifier(m_Logger);
        m_ProtectionsNotifier = new ProtectionsNotifier(m_Obfuscation, m_Logger);
        m_ObfuscationAttributesStripper = new ObfuscationAttributesStripper(m_Obfuscation,
            m_ObfuscationAttributeResolver, m_ObfuscateAssemblyAttributeResolver);
        m_ObfuscationAttributesStripNotifier = new ObfuscationAttributesStripNotifier(m_Logger);
    }

    public async Task ProtectAsync()
    {
        m_Context.ThrowIfCancellationRequested();

        m_InvokablePipeline.OnFail += onFail;

        await m_InvokablePipeline.InvokeAsync(outputProtectionsAsync);
        await m_InvokablePipeline.InvokeAsync(startTimeCounterAsync);
        await m_InvokablePipeline.InvokeAsync(outputFrameworkInformationAsync);
        await m_InvokablePipeline.InvokeAsync(resolveDependenciesAsync);
        await m_InvokablePipeline.InvokeAsync(expandMacrosAsync);
        await m_InvokablePipeline.InvokeAsync(protectAsync);
        await m_InvokablePipeline.InvokeAsync(optimizeMacrosAsync);
        await m_InvokablePipeline.InvokeAsync(stripObfuscationAttributesAsync);
        await m_InvokablePipeline.InvokeAsync(createPEImageAsync);
        await m_InvokablePipeline.InvokeAsync(outputPEImageBuildErrorsAsync);
        await m_InvokablePipeline.InvokeAsync(writeModuleAsync);
        await m_InvokablePipeline.InvokeAsync(packAsync);
        await m_InvokablePipeline.InvokeAsync(outputElapsedTimeAsync);
    }

    private Task<bool> outputProtectionsAsync(ProtectionContext context)
    {
        m_ProtectionsNotifier.Notify(m_ProtectionsSort);
        return Task.FromResult(true);
    }
    private Task<bool> startTimeCounterAsync(ProtectionContext context)
    {
        _startTime = Stopwatch.GetTimestamp();
        return Task.FromResult(true);
    }
    private Task<bool> outputFrameworkInformationAsync(ProtectionContext context)
    {
        m_Logger.Information(RuntimeUtilities.GetFrameworkInformation().ToString());
        return Task.FromResult(true);
    }
    private Task<bool> resolveDependenciesAsync(ProtectionContext context)
    {
        var assemblyResolve = new AssemblyResolver().Resolve(context.BitMonoContext.DependenciesData, context);
        foreach (var reference in assemblyResolve.ResolvedReferences)
        {
            m_Logger.Information("Successfully resolved dependency: {0}", reference.FullName);
        }
        foreach (var reference in assemblyResolve.FailedToResolveReferences)
        {
            m_Logger.Warning("Failed to resolve dependency: {0}", reference.FullName);
        }
        if (assemblyResolve.Succeed == false)
        {
            if (m_Obfuscation.FailOnNoRequiredDependency)
            {
                m_Logger.Fatal("Please, specify needed dependencies, or set in obfuscation.json FailOnNoRequiredDependency to false");
                return Task.FromResult(false);
            }
        }
        return Task.FromResult(true);
    }
    private Task<bool> expandMacrosAsync(ProtectionContext context)
    {
        foreach (var method in m_Context.Module.FindDefinitions().OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is { } body)
            {
                body.Instructions.ExpandMacros();
            }
        }
        return Task.FromResult(true);
    }
    private async Task<bool> protectAsync(ProtectionContext context)
    {
        foreach (var protection in m_ProtectionsSort.SortedProtections)
        {
            context.ThrowIfCancellationRequested();

            await protection.ExecuteAsync(m_Context, createProtectionParameters(protection));
            m_ProtectionExecutionNotifier.Notify(protection);
        }
        foreach (var pipeline in m_ProtectionsSort.Pipelines)
        {
            context.ThrowIfCancellationRequested();

            await pipeline.ExecuteAsync(context, createProtectionParameters(pipeline));
            m_ProtectionExecutionNotifier.Notify(pipeline);

            foreach (var phase in pipeline.PopulatePipeline())
            {
                context.ThrowIfCancellationRequested();

                await phase.ExecuteAsync(context, createProtectionParameters(phase));
                m_ProtectionExecutionNotifier.Notify(phase);
            }
        }
        return true;
    }
    private Task<bool> optimizeMacrosAsync(ProtectionContext context)
    {
        foreach (var method in context.Module.FindDefinitions().OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is { } body)
            {
                body.Instructions.OptimizeMacros();
            }
        }
        return Task.FromResult(true);
    }
    private Task<bool> stripObfuscationAttributesAsync(ProtectionContext context)
    {
        var obfuscationAttributesStrip = m_ObfuscationAttributesStripper.Strip(context, m_ProtectionsSort);
        m_ObfuscationAttributesStripNotifier.Notify(obfuscationAttributesStrip);
        return Task.FromResult(true);
    }
    private Task<bool> createPEImageAsync(ProtectionContext context)
    {
        _imageBuild = context.PEImageBuilder.CreateImage(context.Module);
        return Task.FromResult(true);
    }
    private Task<bool> outputPEImageBuildErrorsAsync(ProtectionContext context)
    {
        if (m_Obfuscation.OutputPEImageBuildErrors)
        {
            if (_imageBuild.DiagnosticBag.HasErrors)
            {
                m_Logger.Warning("{0} errors were registered while building the PE", _imageBuild.DiagnosticBag.Exceptions.Count);
                foreach (var exception in _imageBuild.DiagnosticBag.Exceptions)
                {
                    m_Logger.Error(exception, "Error while building the PE!");
                }
            }
        }
        return Task.FromResult(true);
    }
    private async Task<bool> writeModuleAsync(ProtectionContext context)
    {
        try
        {
            var memoryStream = new MemoryStream();
            var fileBuilder = new ManagedPEFileBuilder();
            fileBuilder
                .CreateFile(_imageBuild.ConstructedImage)
                .Write(memoryStream);
            await m_DataWriter.WriteAsync(context.BitMonoContext.OutputFile, memoryStream.ToArray());
            m_Logger.Information("Protected module`s saved in {0}", context.BitMonoContext.OutputDirectoryName);
        }
        catch (Exception ex)
        {
            m_Logger.Fatal(ex, "An error occured while writing the module!");
            return false;
        }
        return true;
    }
    private async Task<bool> packAsync(ProtectionContext context)
    {
        foreach (var packer in m_ProtectionsSort.Packers)
        {
            context.ThrowIfCancellationRequested();

            await packer.ExecuteAsync(m_Context, createProtectionParameters(packer));
            m_ProtectionExecutionNotifier.Notify(packer);
        }
        return true;
    }
    private Task<bool> outputElapsedTimeAsync(ProtectionContext context)
    {
        var elapsedTime = StopwatchUtilities.GetElapsedTime(_startTime, Stopwatch.GetTimestamp());
        m_Logger.Information("Since obfuscation elapsed: {0}", elapsedTime.ToString());
        return Task.FromResult(true);
    }
    private void onFail()
    {
        m_Logger.Fatal("Obfuscation stopped! Something went wrong!");
    }
    private ProtectionParameters createProtectionParameters(IProtection target)
    {
        return new ProtectionParametersFactory(m_MemberResolver, m_MemberResolvers).Create(target, m_Context.Module);
    }
}