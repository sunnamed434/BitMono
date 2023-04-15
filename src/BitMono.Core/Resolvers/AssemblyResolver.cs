#pragma warning disable CS8602
namespace BitMono.Core.Resolvers;

public static class AssemblyResolver
{
    public static AssemblyResolve Resolve(IEnumerable<byte[]> dependenciesData, ProtectionContext context)
    {
        context.ThrowIfCancellationRequested();

        var resolvedReferences = new List<AssemblyReference>();
        var failedToResolveReferences = new List<AssemblyReference>();
        var signatureComparer = new SignatureComparer(SignatureComparisonFlags.AcceptNewerVersions);

        foreach (var originalReference in context.Module.AssemblyReferences)
        {
            context.ThrowIfCancellationRequested();

            var resolved = false;
            if (context.AssemblyResolver.HasCached(originalReference))
            {
                resolvedReferences.Add(originalReference);
                continue;
            }
            if (failedToResolveReferences.Contains(originalReference) || resolvedReferences.Contains(originalReference))
            {
                continue;
            }

            foreach (var data in dependenciesData)
            {
                context.ThrowIfCancellationRequested();

                try
                {
                    var definition = AssemblyDefinition.FromBytes(data);
                    if (signatureComparer.Equals(originalReference, definition))
                    {
                        context.AssemblyResolver.Resolve(definition);
                        resolvedReferences.Add(originalReference);
                        resolved = true;
                        break;
                    }
                }
                catch (BadImageFormatException)
                {
                }
            }

            if (resolved == false)
            {
                failedToResolveReferences.Add(originalReference);
            }
        }

        var succeed = failedToResolveReferences.Count == 0;
        return new AssemblyResolve
        {
            ResolvedReferences = resolvedReferences,
            FailedToResolveReferences = failedToResolveReferences,
            Succeed = succeed
        };
    }
}