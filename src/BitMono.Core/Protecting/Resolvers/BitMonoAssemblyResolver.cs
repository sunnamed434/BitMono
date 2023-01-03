namespace BitMono.Core.Protecting.Resolvers;

public class BitMonoAssemblyResolver
{
    public AssemblyResolve Resolve(IEnumerable<byte[]> dependenciesData, ProtectionContext context)
    {
        context.ThrowIfCancellationRequested();
        var resolvedReferences = new HashSet<AssemblyReference>();
        var failedToResolveReferences = new HashSet<AssemblyReference>();

        var resolveSucceed = true;
        foreach (var originalReference in context.Module.AssemblyReferences)
        {
            context.ThrowIfCancellationRequested();

            foreach (var data in dependenciesData)
            {
                context.ThrowIfCancellationRequested();

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
                catch (ArgumentException)
                {
                    resolveSucceed = false;
                    failedToResolveReferences.Add(originalReference);
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