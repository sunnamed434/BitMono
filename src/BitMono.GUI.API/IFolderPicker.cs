namespace BitMono.GUI.API;

public interface IFolderPicker
{
    Task<string> PickAsync();
}