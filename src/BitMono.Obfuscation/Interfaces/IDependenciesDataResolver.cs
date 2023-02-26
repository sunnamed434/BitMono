namespace BitMono.Obfuscation.Interfaces;

public interface IDependenciesDataResolver
{
    IEnumerable<byte[]> Resolve();
}