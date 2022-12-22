namespace BitMono.API.Protecting.Renaming;

public interface IRenamer
{
    string RenameUnsafely();
    void Rename(object @object);
    void Rename(params object[] objects);
    void RemoveNamespace(object @object);
    void RemoveNamespace(params object[] objects);
}