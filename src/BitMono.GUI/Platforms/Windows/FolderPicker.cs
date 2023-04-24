using WinRT.Interop;
using WindowsFolderPicker = Windows.Storage.Pickers.FolderPicker;

namespace BitMono.GUI.Platforms.Windows
{
    public class FolderPicker : IFolderPicker
    {
        public async Task<string> PickAsync()
        {
            var folderPicker = new WindowsFolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            var hwnd = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).WindowHandle;

            InitializeWithWindow.Initialize(folderPicker, hwnd);

            var result = await folderPicker.PickSingleFolderAsync();
            return result?.Path ?? string.Empty;
        }
    }
}