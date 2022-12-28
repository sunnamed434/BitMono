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
        // temp comment
        //foreach (var dependencyData in dependenciesData)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //
        //    try
        //    {
        //        context.AssemblyResolver.AddToCache(context.Module.Assembly, AssemblyDefinition.FromBytes(dependencyData));
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //}
        foreach (var assemblyReference in context.Module.AssemblyReferences)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                m_Logger.Information("Resolving assembly: " + assemblyReference.Name);
                var assembly = context.AssemblyResolver.Resolve(assemblyReference);
                if (assembly == null)
                {
                    m_Logger.Error("Failed to resolve dependency {0}", assemblyReference.FullName);
                }
            }
            catch (Exception ex)
            {
                resolvingSucceed = false;
                m_Logger.Error("Failed to resolve dependency {0}, error: {1}", assemblyReference.FullName, ex.Message);
            }
        }
        return resolvingSucceed;
    }
}