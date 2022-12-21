namespace BitMono.API.Protecting.Renaming;

public interface IRenamer
{
    string RenameUnsafely();
    void Rename(IMetadataMember metadataMember);
    void Rename(params IMetadataMember[] metadataMembers);
}