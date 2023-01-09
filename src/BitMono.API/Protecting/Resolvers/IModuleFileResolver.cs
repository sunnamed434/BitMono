namespace BitMono.API.Protecting.Resolvers;

public interface IModuleFileResolver
{
    public Task<string> ResolveAsync();
}