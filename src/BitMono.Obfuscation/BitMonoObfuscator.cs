using AsmResolver.PE.DotNet.Builder;

namespace BitMono.Obfuscation;

public class BitMonoObfuscator
{
    private readonly ICollection<IProtection> m_Protections;
    private readonly ICollection<IPacker> m_Packers;
    private IEnumerable<IMemberDefinitionfResolver> m_DnlibDefResolvers;
    private readonly ProtectionContext m_ProtectionContext;
    private readonly IDataWriter m_DataWriter;
    private DnlibDefsResolver m_DnlibDefsResolver;
    private readonly ILogger m_Logger;

    public BitMonoObfuscator(
        IEnumerable<IMemberDefinitionfResolver> dnlibDefResolvers,
        ICollection<IProtection> protections,
        ICollection<IPacker> packers,
        ProtectionContext protectionContext,
        IDataWriter dataWriter,
        ILogger logger)
    {
        m_DnlibDefResolvers = dnlibDefResolvers;
        m_Protections = protections;
        m_Packers = packers;
        m_ProtectionContext = protectionContext;
        m_DataWriter = dataWriter;
        m_DnlibDefsResolver = new DnlibDefsResolver();
        m_Logger = logger.ForContext<BitMonoObfuscator>();
    }

    public async Task StartAsync(CancellationTokenSource cancellationTokenSource)
    {
        var cancellationToken = cancellationTokenSource.Token;
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var methodDefinition in m_ProtectionContext.Module.FindDefinitions().OfType<MethodDefinition>())
        {
            methodDefinition.CilMethodBody.Instructions.ExpandMacros();
            methodDefinition.CilMethodBody.Instructions.OptimizeMacros();

        }

        foreach (var protection in m_Protections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ((protection is IPipelineStage) == false)
            {
                var protectionName = protection.GetName();
                var protectionParameters = new ProtectionParametersCreator(m_DnlibDefsResolver, m_DnlibDefResolvers).Create(protectionName, m_ProtectionContext.Module);
                await protection.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
                m_Logger.Information("{0} -> OK!", protectionName);
            }
        }

        /*try
        {
            Write(m_ProtectionContext.Module, m_ProtectionContext.PEImageBuilder);
        }
        catch (Exception ex)
        {
            m_Logger.Fatal(ex, "Failed to write module!");
            cancellationTokenSource.Cancel();
            return;
        }*/

        foreach (var protection in m_Protections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (protection is IPipelineStage stage)
            {
                if (stage.Stage == PipelineStages.ModuleWritten)
                {
                    var protectionName = protection.GetName();
                    var protectionParameters = new ProtectionParametersCreator(m_DnlibDefsResolver, m_DnlibDefResolvers).Create(protectionName, m_ProtectionContext.Module);
                    await protection.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
                    m_Logger.Information("{0} -> OK!", protectionName);
                }
            }

            if (protection is IPipelineProtection pipelineProtection)
            {
                foreach (var protectionPhase in pipelineProtection.PopulatePipeline())
                {
                    if (protectionPhase.Item2 == PipelineStages.ModuleWritten)
                    {
                        var protectionName = protection.GetName();
                        var protectionParameters = new ProtectionParametersCreator(m_DnlibDefsResolver, m_DnlibDefResolvers).Create(protectionName, m_ProtectionContext.Module);
                        await protectionPhase.Item1.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
                        m_Logger.Information("{0} -> Pipeline OK!", protectionName);
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
            var protectionParameters = new ProtectionParametersCreator(m_DnlibDefsResolver, m_DnlibDefResolvers).Create(packerName, m_ProtectionContext.Module);
            await packer.ExecuteAsync(m_ProtectionContext, protectionParameters, cancellationToken);
            m_Logger.Information("{0} -> Packer OK", packerName);
        }
    }

    public void Write(ModuleDefinition moduleDefinition, IPEImageBuilder peImageBuilder)
    {
        var memoryStream = new MemoryStream();
        moduleDefinition.Write(memoryStream, peImageBuilder);
        m_ProtectionContext.ModuleOutput = memoryStream.ToArray();
    }
}