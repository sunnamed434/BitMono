#pragma warning disable CS8602
namespace BitMono.Core.Resolvers;

[SuppressMessage("ReSharper", "InvertIf")]
public static class AssemblyResolver
{
    public static AssemblyResolve Resolve(IEnumerable<byte[]> dependenciesData, StarterContext context)
    {
        context.ThrowIfCancellationRequested();

        var resolvedReferences = new List<AssemblyReference>();
        var failedToResolveReferences = new List<AssemblyReference>();
        var signatureComparer = new SignatureComparer(SignatureComparisonFlags.AcceptNewerVersions);
        var runtimeContext = context.Module.RuntimeContext;

        foreach (var originalReference in context.Module.AssemblyReferences)
        {
            context.ThrowIfCancellationRequested();

            var resolved = false;
            // AsmResolver 6.0 moved assembly caching into RuntimeContext (IAssemblyResolver.HasCached was removed).
            // A reference already loaded into the context counts as resolved.
            if (runtimeContext.GetLoadedAssemblies().Any(loaded => signatureComparer.Equals(originalReference, loaded)))
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
                        // Register the matched dependency into the context so later resolution finds it.
                        runtimeContext.LoadAssembly(data);
                        resolvedReferences.Add(originalReference);
                        resolved = true;
                        break;
                    }
                }
                catch (BadImageFormatException)
                {
                    // ignored
                }
                catch (EndOfStreamException)
                {
                    // ignored
                }
            }

            if (!resolved)
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