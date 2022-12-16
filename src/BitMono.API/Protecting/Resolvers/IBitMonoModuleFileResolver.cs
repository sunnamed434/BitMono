namespace BitMono.API.Protecting.Resolvers;

public interface IBitMonoModuleFileResolver
{
    public Task<string> ResolveAsync();
}