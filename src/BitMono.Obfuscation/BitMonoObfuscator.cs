namespace BitMono.Obfuscation;

public class BitMonoObfuscator
{
    private readonly ProtectionContext m_ProtectionContext;
    private readonly IEnumerable<IMemberResolver> m_MemberResolvers;
    private readonly ProtectionsSort m_ProtectionsSort;
    private readonly IDataWriter m_DataWriter;
    private readonly ObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private readonly IConfiguration m_ObfuscationConfiguration;
    private readonly IInvokablePipeline m_InvokablePipeline;
    private MembersResolver m_MemberResolver;
    private readonly ILogger m_Logger;

    public BitMonoObfuscator(
        ProtectionContext protectionContext,
        IEnumerable<IMemberResolver> memberResolvers,
        ProtectionsSort protectionsSortResult,
        IDataWriter dataWriter,
        ObfuscationAttributeResolver obfuscationAttributeResolver,
        IBitMonoObfuscationConfiguration obfuscationConfiguration,
        ILogger logger)
    {
        m_MemberResolvers = memberResolvers;
        m_ProtectionsSort = protectionsSortResult;
        m_ProtectionContext = protectionContext;
        m_DataWriter = dataWriter;
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_ObfuscationConfiguration = obfuscationConfiguration.Configuration;
        m_InvokablePipeline = new InvokablePipeline(m_ProtectionContext);
        m_MemberResolver = new MembersResolver();
        m_Logger = logger.ForContext<BitMonoObfuscator>();
    }

    public async Task StartAsync()
    {
        m_ProtectionContext.CancellationToken.ThrowIfCancellationRequested();

        m_InvokablePipeline.OnFail += onFail;

        await m_InvokablePipeline.InvokeAsync(outputFrameworkInformationAsync);
        await m_InvokablePipeline.InvokeAsync(resolveDependenciesAsync);
        await m_InvokablePipeline.InvokeAsync(expandMacrosAsync);
        await m_InvokablePipeline.InvokeAsync(protectAsync);
        await m_InvokablePipeline.InvokeAsync(optimizeMacrosAsync);
        await m_InvokablePipeline.InvokeAsync(stripObfuscationAttributesAsync);
        await m_InvokablePipeline.InvokeAsync(writeModuleAsync);
        await m_InvokablePipeline.InvokeAsync(packAsync);
    }

    private Task<bool> outputFrameworkInformationAsync(ProtectionContext context)
    {
        m_Logger.Information(RuntimeUtilities.GetFrameworkInformation().ToString());
        return Task.FromResult(true);
    }
    private Task<bool> resolveDependenciesAsync(ProtectionContext context)
    {
        var assemblyResolve = new BitMonoAssemblyResolver().Resolve(context.BitMonoContext.DependenciesData, context);
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
            if (m_ObfuscationConfiguration.GetValue<bool>(nameof(Shared.Models.Obfuscation.FailOnNoRequiredDependency)))
            {
                m_Logger.Fatal("Please, specify needed dependencies, or set in obfuscation.json FailOnNoRequiredDependency to false");
                return Task.FromResult(false);
            }
        }
        return Task.FromResult(true);
    }
    private Task<bool> expandMacrosAsync(ProtectionContext context)
    {
        foreach (var method in m_ProtectionContext.Module.FindDefinitions().OfType<MethodDefinition>())
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
            context.CancellationToken.ThrowIfCancellationRequested();

            var protectionName = protection.GetName();
            var protectionParameters = new ProtectionParametersCreator(m_MemberResolver, m_MemberResolvers).Create(protection, m_ProtectionContext.Module);
            await protection.ExecuteAsync(m_ProtectionContext, protectionParameters);
            m_Logger.Information("{0} -> OK", protectionName);
        }
        foreach (var pipeline in m_ProtectionsSort.Pipelines)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (pipeline is IProtection protection)
            {
                var protectionName = protection.GetName();
                var protectionParameters = new ProtectionParametersCreator(m_MemberResolver, m_MemberResolvers).Create(protection, m_ProtectionContext.Module);
                await protection.ExecuteAsync(context, protectionParameters);
                m_Logger.Information("{0} -> Pipeline Protection OK", protectionName);
            }
            foreach (var phase in pipeline.PopulatePipeline())
            {
                var protectionName = phase.GetName();
                var protectionParameters = new ProtectionParametersCreator(m_MemberResolver, m_MemberResolvers).Create(phase, m_ProtectionContext.Module);
                await phase.ExecuteAsync(context, protectionParameters);
                m_Logger.Information("{0} -> Pipeline Phase Protection OK", protectionName);
            }
        }
        return true;
    }
    private async Task<bool> writeModuleAsync(ProtectionContext context)
    {
        try
        {
            var memoryStream = new MemoryStream();
            var image = context.PEImageBuilder.CreateImage(context.Module).ConstructedImage;
            new ManagedPEFileBuilder().CreateFile(image).Write(memoryStream);
            context.ModuleOutput = memoryStream.ToArray();
            await m_DataWriter.WriteAsync(context.BitMonoContext.OutputFile, context.ModuleOutput);
        }
        catch (Exception ex)
        {
            m_Logger.Fatal(ex, "Error while writing the file!");
            return false;
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
        foreach (var customAttribute in context.Module.FindDefinitions().OfType<IHasCustomAttribute>())
        {
            foreach (var protection in m_ProtectionsSort.ProtectionsResolve.FoundProtections)
            {
                if (m_ObfuscationAttributeResolver.Resolve(protection.GetName(), customAttribute, out CustomAttributeResolve attributeResolve))
                {
                    if (customAttribute.CustomAttributes.Remove(attributeResolve.CustomAttribute))
                    {
                        m_Logger.Information("Successfully stripped obfuscation attribute");
                    }
                    else
                    {
                        m_Logger.Warning("Failed to stip obfuscation attribute");
                    }
                }
            }
        }
        return Task.FromResult(true);
    }
    private async Task<bool> packAsync(ProtectionContext context)
    {
        foreach (var packer in m_ProtectionsSort.Packers)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var packerName = packer.GetName();
            var protectionParameters = new ProtectionParametersCreator(m_MemberResolver, m_MemberResolvers).Create(packer, m_ProtectionContext.Module);
            await packer.ExecuteAsync(m_ProtectionContext, protectionParameters);
            m_Logger.Information("{0} -> Packer OK", packerName);
        }
        return true;
    }
    private void onFail()
    {
        m_Logger.Fatal("Obfuscation stopped! Something went wrong!");
    }
}