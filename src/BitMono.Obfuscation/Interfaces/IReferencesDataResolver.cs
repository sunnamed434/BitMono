namespace BitMono.Obfuscation.Interfaces;

public interface IReferencesDataResolver
{
    List<byte[]> Resolve(ModuleDefinition module);
}