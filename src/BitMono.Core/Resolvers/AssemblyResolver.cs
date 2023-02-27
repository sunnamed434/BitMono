#pragma warning disable CS8602
namespace BitMono.Core.Resolvers;

public static class AssemblyResolver
{
    public static AssemblyResolve Resolve(IEnumerable<byte[]> dependenciesData, ProtectionContext context)
    {
        context.ThrowIfCancellationRequested();

        var resolvedReferences = new List<AssemblyReference>();
        var failedToResolveReferences = new List<AssemblyReference>();
        var badImageReferences = new List<AssemblyReference>();
        var signatureComparer = new SignatureComparer(SignatureComparisonFlags.AcceptNewerVersions);

        foreach (var originalReference in context.Module.AssemblyReferences)
        {
            context.ThrowIfCancellationRequested();

            foreach (var data in dependenciesData)
            {
                context.ThrowIfCancellationRequested();

                try
                {
                    var definition = AssemblyDefinition.FromBytes(data);
                    if (context.AssemblyResolver.HasCached(originalReference) == false && signatureComparer.Equals(originalReference, definition))
                    {
                        context.AssemblyResolver.AddToCache(originalReference, definition);
                    }
                }
                catch (BadImageFormatException ex)
                {
                    Console.WriteLine("originalRef: " + originalReference.Name + ", " + ex.ToString());
                    badImageReferences.Add(originalReference);
                }
            }
        }
        var succeed = true;
        foreach (var originalReference in context.Module.AssemblyReferences)
        {
            if (context.AssemblyResolver.HasCached(originalReference))
            {
                resolvedReferences.Add(originalReference);
            }
            else
            {
                failedToResolveReferences.Add(originalReference);
                succeed = false;
            }
        }
        return new AssemblyResolve
        {
            ResolvedReferences = resolvedReferences,
            FailedToResolveReferences = failedToResolveReferences,
            BadImageReferences = badImageReferences,
            Succeed = succeed
        };
    }
}