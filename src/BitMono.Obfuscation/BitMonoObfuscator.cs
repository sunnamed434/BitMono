namespace BitMono.Obfuscation;

public class BitMonoObfuscator
{
    private readonly ProtectionContext m_ProtectionContext;
    private IEnumerable<IMemberResolver> m_MemberDefinitionResolvers;
    private readonly ProtectionsSort m_ProtectionsSort;
    private readonly ICollection<IPacker> m_Packers;
    private readonly ICollection<IProtection> m_Protections;
    private readonly IDataWriter m_DataWriter;
    private readonly ObfuscationAttributeResolver m_ObfuscationAttributeResolver;
    private MembersResolver m_MemberResolver;
    private readonly ILogger m_Logger;

    public BitMonoObfuscator(
        ProtectionContext protectionContext,
        IEnumerable<IMemberResolver> memberDefinitionResolvers,
        ProtectionsSort protectionsSortResult,
        IDataWriter dataWriter,
        ObfuscationAttributeResolver obfuscationAttributeResolver,
        ILogger logger)
    {
        m_MemberDefinitionResolvers = memberDefinitionResolvers;
        m_ProtectionsSort = protectionsSortResult;
        m_Protections = m_ProtectionsSort.SortedProtections;
        m_Packers = m_ProtectionsSort.Packers;
        m_ProtectionContext = protectionContext;
        m_DataWriter = dataWriter;
        m_ObfuscationAttributeResolver = obfuscationAttributeResolver;
        m_MemberResolver = new MembersResolver();
        m_Logger = logger.ForContext<BitMonoObfuscator>();
    }

    public async Task StartAsync(CancellationTokenSource cancellationTokenSource)
    {
        var cancellationToken = cancellationTokenSource.Token;
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var method in m_ProtectionContext.Module.FindDefinitions().OfType<MethodDefinition>())
        {
            if (method.CilMethodBody is { } body)
            {
                body.Instructions.ExpandMacros();
                body.Instructions.OptimizeMacros();
            }
        }
        foreach (var protection in m_Protections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ((protection is IPipelineStage) == false)
            {
                var protectionName = protection.GetName();
                var protectionParameters = new ProtectionParametersCreator(m_MemberResolver, m_MemberDefinitionResolvers).Create(protectionName, m_ProtectionContext.Module);
                await protection.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
                m_Logger.Information("{0} -> OK", protectionName);
            }
        }
        foreach (var protection in m_Protections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (protection is IPipelineStage stage)
            {
                if (stage.Stage == PipelineStages.ModuleWrite)
                {
                    var protectionName = protection.GetName();
                    var protectionParameters = new ProtectionParametersCreator(m_MemberResolver, m_MemberDefinitionResolvers).Create(protectionName, m_ProtectionContext.Module);
                    await protection.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
                    m_Logger.Information("{0} -> OK", protectionName);
                }
            }

            if (protection is IPipelineProtection pipelineProtection)
            {
                foreach (var protectionPhase in pipelineProtection.PopulatePipeline())
                {
                    if (protectionPhase.Item2 == PipelineStages.ModuleWrite)
                    {
                        var protectionName = protection.GetName();
                        var protectionParameters = new ProtectionParametersCreator(m_MemberResolver, m_MemberDefinitionResolvers).Create(protectionName, m_ProtectionContext.Module);
                        await protectionPhase.Item1.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
                        m_Logger.Information("{0} -> Pipeline OK", protectionName);
                    }
                }
            }
        }

        foreach (var customAttribute in m_ProtectionContext.Module.FindDefinitions().OfType<IHasCustomAttribute>())
        {
            foreach (var protection in m_ProtectionsSort.FoundProtections)
            {
                if (m_ObfuscationAttributeResolver.Resolve(protection.GetName(), customAttribute, out CustomAttributeResolve attributeResolve))
                {
                    if (customAttribute.CustomAttributes.Remove(attributeResolve.CustomAttribute))
                    {
                        m_Logger.Information("Successfully removed obfuscation attribute");
                    }
                    else
                    {
                        m_Logger.Warning("Failed to remove obfuscation attribute");
                    }
                }
            }
        }

        try
        {
            var memoryStream = new MemoryStream();
            var image = m_ProtectionContext.PEImageBuilder.CreateImage(m_ProtectionContext.Module).ConstructedImage;
            new ManagedPEFileBuilder().CreateFile(image).Write(memoryStream);
            m_ProtectionContext.ModuleOutput = memoryStream.ToArray();
            await m_DataWriter.WriteAsync(m_ProtectionContext.BitMonoContext.OutputFile, m_ProtectionContext.ModuleOutput);
        }
        catch (Exception ex)
        {
            m_Logger.Fatal(ex, "Error while writing file!");
            cancellationTokenSource.Cancel();
            return;
        }

        foreach (var packer in m_Packers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var packerName = packer.GetName();
            var protectionParameters = new ProtectionParametersCreator(m_MemberResolver, m_MemberDefinitionResolvers).Create(packerName, m_ProtectionContext.Module);
            await packer.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
            m_Logger.Information("{0} -> Packer OK", packerName);
        }
    }
}