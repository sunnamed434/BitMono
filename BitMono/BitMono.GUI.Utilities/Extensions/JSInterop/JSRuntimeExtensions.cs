using Microsoft.AspNetCore.Components;
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
        public static async Task ScrollToEndAsync(this IJSRuntime source, ElementReference textAreaReference)
        {
            await source.InvokeVoidAsync("scrollToEnd", textAreaReference);
        }
        public static async Task CopyTextToClipboardAsync(this IJSRuntime source, string content)
        {
            await source.InvokeVoidAsync("navigator.clipboard.writeText", content);
        }
        public static async Task AddTooltipsAsync(this IJSRuntime source)
        {
            await source.InvokeVoidAsync("addTooltips");
        }
    }
}