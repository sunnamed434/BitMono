namespace BitMono.Obfuscation.Referencing;

public interface IReferencesDataResolver
{
    List<byte[]> Resolve(ModuleDefinition module);
}