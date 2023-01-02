namespace BitMono.Core.Protecting.Resolvers;

public class BitMonoAssemblyResolver
{
    private readonly ILogger m_Logger;

    public BitMonoAssemblyResolver(ILogger logger)
    {
        m_Logger = logger.ForContext<BitMonoAssemblyResolver>();
    }

    public AssemblyResolve Resolve(IEnumerable<byte[]> dependenciesData, ProtectionContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resolvedReferences = new HashSet<AssemblyReference>();
        var failedToResolveReferences = new HashSet<AssemblyReference>();

        var resolveSucceed = true;
        foreach (var originalReference in context.Module.AssemblyReferences)
        {
            foreach (var data in dependenciesData)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var defenition = AssemblyDefinition.FromBytes(data);
                    if (context.AssemblyResolver.HasCached(defenition) == false)
                    {
                        context.AssemblyResolver.AddToCache(originalReference, defenition);
                    }
                    var resolved = context.AssemblyResolver.Resolve(defenition);
                    if (resolved != null)
                    {
                        resolvedReferences.Add(originalReference);
                    }
                    else
                    {
                        failedToResolveReferences.Add(originalReference);
                    }
                }
                catch (ArgumentException ex)
                {
                    resolveSucceed = false;
                    failedToResolveReferences.Add(originalReference);
                    m_Logger.Debug("Failed to resolve dependency, error: {0}", ex.ToString());
                }
            }
        }
        return new AssemblyResolve
        {
            ResolvedReferences = resolvedReferences,
            FailedToResolveReferences = failedToResolveReferences.Except(resolvedReferences).ToHashSet(),
            Succeed = resolveSucceed,
        };
    }
}