namespace BitMono.GUI;

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
        builder.Services.AddScoped<AlertsContainer>();
        builder.Services.AddSingleton<IStoringProtections, StoringProtections>();
        var handlerLogEventSink = new HandlerLogEventSink();
        builder.Services.AddSingleton(handlerLogEventSink);

        const string ProtectionsFile = "BitMono.Protections.dll";
        Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ProtectionsFile));
        builder.ConfigureContainer(new AutofacServiceProviderFactory(), configure =>
        {
            configure.RegisterModule(new BitMonoModule(configureLogger =>
            {
                configureLogger.WriteTo.Async(configure =>
                {
                    configure.Sink(handlerLogEventSink);
                });
            }));
        });
        return builder.Build();
    }
}