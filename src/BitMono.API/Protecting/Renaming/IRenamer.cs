namespace BitMono.API.Protecting.Renaming;

public interface IRenamer
{
    string RenameUnsafely();
    void Rename(IMemberDefinition memberDefinition);
    void Rename(params IMemberDefinition[] memberDefinitions);
}