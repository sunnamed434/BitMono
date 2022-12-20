namespace BitMono.Core.Protecting.Resolvers;

public class BitMonoAssemblyResolver
{
    private readonly ILogger m_Logger;

    public BitMonoAssemblyResolver(ILogger logger)
    {
        m_Logger = logger.ForContext<BitMonoAssemblyResolver>();
    }

    public bool Resolve(IEnumerable<byte[]> dependenciesData, ProtectionContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var resolvingSucceed = true;
        foreach (var dependencyData in dependenciesData)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //context.AssemblyResolver.AddToCache(AssemblyDefinition.FromBytes(dependencyData), context.Module.Assembly);
        }
        foreach (var assemblyReference in context.Module.AssemblyReferences)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                m_Logger.Information("Resolving assembly: " + assemblyReference.Name);
                context.AssemblyResolver.Resolve(assemblyReference);
            }
            catch (Exception ex)
            {
                resolvingSucceed = false;
                m_Logger.Error("Failed to resolve dependency {0}, message: ", assemblyReference.FullName, ex.Message);
            }
        }
        return resolvingSucceed;
    }
}