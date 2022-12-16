namespace BitMono.Obfuscation.API;

public interface IDependenciesDataResolver
{
    IEnumerable<byte[]> Resolve();
}