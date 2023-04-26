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
        builder.Services.AddScoped<AlertsContainer>();
        builder.Services.AddSingleton<IStoringProtections, StoringProtections>();
        var handlerLogEventSink = new HandlerLogEventSink();
        builder.Services.AddSingleton(handlerLogEventSink);

        builder.ConfigureContainer(new AutofacServiceProviderFactory(), configure =>
        {
            configure.RegisterModule(new BitMonoModule(
                configureContainer => configureContainer.AddProtections(),
                configureServices => configureServices.AddConfigurations(),
                configureLogger =>
                {
                    configureLogger.WriteTo.Async(configureSink =>
                    {
                        configureSink.Sink(handlerLogEventSink);
                    });
                }));
        });
        return builder.Build();
    }
}