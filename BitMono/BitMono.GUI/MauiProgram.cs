using Autofac;
using Autofac.Extensions.DependencyInjection;
using BitMono.GUI.API;
using BitMono.GUI.Data;
using BitMono.Host.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using System;
using System.IO;
using System.Reflection;

namespace BitMono.GUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#endif
#if WINDOWS
            builder.Services.AddTransient<IFolderPicker, Platforms.Windows.FolderPicker>();
#endif
            builder.Services.AddSingleton<IStoringProtections, StoringProtections>();

            const string ProtectionsFile = "BitMono.Protections.dll";
            Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ProtectionsFile));
            builder.ConfigureContainer(new AutofacServiceProviderFactory(), configure =>
            {
                configure.RegisterModule(new BitMonoModule());
            });

            return builder.Build();
        }
    }
}