namespace BitMono.API.Protecting.Renaming;

public interface IRenamer
{
    string RenameUnsafely();
    void Rename(IMetadataMember member);
    void Rename(params IMetadataMember[] members);
    void RemoveNamespace(IMetadataMember member);
    void RemoveNamespace(params IMetadataMember[] members);
}