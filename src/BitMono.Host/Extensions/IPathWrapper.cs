namespace BitMono.Host.Extensions;

public interface IPathWrapper
{
    bool IsDirectory(string path);
    string GetFileName(string path);
    string GetDirectoryName(string path);
}