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

            context.AssemblyResolver.AddToCache(AssemblyDef.Load(dependencyData));
        }

        foreach (var assemblyRef in context.ModuleDefMD.GetAssemblyRefs())
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                m_Logger.Information("Resolving assembly: " + assemblyRef.Name);
                context.ModuleCreationOptions.Context.AssemblyResolver.ResolveThrow(assemblyRef, context.ModuleDefMD);
            }
            catch (Exception ex)
            {
                resolvingSucceed = false;
                m_Logger.Error("Failed to resolve dependency {0}, message: ", assemblyRef.FullName, ex.Message);
            }
        }
        return resolvingSucceed;
    }
}