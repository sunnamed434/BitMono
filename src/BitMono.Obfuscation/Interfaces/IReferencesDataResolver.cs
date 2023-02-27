namespace BitMono.Obfuscation.Interfaces;

public interface IReferencesDataResolver
{
    IEnumerable<byte[]> Resolve(ModuleDefinition module);
}