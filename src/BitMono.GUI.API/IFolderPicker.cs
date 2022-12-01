using System.Threading.Tasks;

namespace BitMono.GUI.API
{
    public interface IFolderPicker
    {
        Task<string> PickAsync();
    }
}