using Microsoft.JSInterop;

namespace BitMono.GUI.Utilities.Extensions.JSInterop
{
    public static class JSRuntimeExtensions
    {
        public static async Task ShowModalAsync(this IJSRuntime source, string modalId)
        {
            await source.InvokeVoidAsync("ShowModal", modalId);
        }
        public static async Task ShowModalStaticAsync(this IJSRuntime source, string modalId)
        {
            await source.InvokeVoidAsync("ShowModalStatic", modalId);
        }
        public static async Task HideModalAsync(this IJSRuntime source, string modalId)
        {
            await source.InvokeVoidAsync("HideModal", modalId);
        }
    }
}