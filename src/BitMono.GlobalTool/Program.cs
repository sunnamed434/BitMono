namespace BitMono.GlobalTool;

internal class Program
{
    private static readonly CancellationTokenSource CancellationTokenSource = new();
    private static CancellationToken CancellationToken => CancellationTokenSource.Token;
    private static readonly string BitMonoFileVersionText =
        $"BitMono v{FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).FileVersion}";
    private static readonly string AsciiArt = $"""

                                                      ___  _ __  __  ___
                                                     / _ )(_) /_/  |/  /__  ___  ___
                                                    / _  / / __/ /|_/ / _ \/ _ \/ _ \
                                                   /____/_/\__/_/  /_/\___/_//_/\___/
                                                   https://github.com/sunnamed434/BitMono
                                                   {BitMonoFileVersionText}
                                               """;

    private static async Task<int> Main(string[] args)
    {
        Console.CancelKeyPress += OnCancelKeyPress;
        var statusCode = KnownReturnStatuses.Success;
        ObfuscationNeeds? needs = null;
        try
        {
            Console.Title = BitMonoFileVersionText;
            
            needs = new ObfuscationNeedsFactory(args).Create(CancellationToken);
            if (needs == null)
            {
                statusCode = KnownReturnStatuses.Failure;
                return statusCode;
            }
            
            var module = new BitMonoModule(
                configureContainer => configureContainer.AddProtections(),
                configureServices => configureServices.AddConfigurations(
                    protectionSettings: needs.ProtectionSettings,
                    criticalsFile: needs.CriticalsFile,
                    obfuscationFile: needs.ObfuscationFile,
                    loggingFile: needs.LoggingFile,
                    protectionsFile: needs.ProtectionsFile,
                    obfuscationSettings: needs.ObfuscationSettings),
                configureLogger => configureLogger.WriteTo.AddConsoleLogger(),
                loggingFile: needs.LoggingFile);

            var app = new BitMonoApplication().RegisterModule(module);
            await using var serviceProvider = await app.BuildAsync(CancellationToken);

            var obfuscation = serviceProvider.GetRequiredService<IOptions<ObfuscationSettings>>().Value;
            var logger = serviceProvider
                .GetRequiredService<ILogger>()
                .ForContext<Program>();

            CancellationToken.ThrowIfCancellationRequested();

            if (obfuscation.ClearCLI)
            {
                Console.Clear();
            }
            logger.Information("File: {0}", needs.FileName);
            logger.Information("Dependencies (libs): {0}", needs.ReferencesDirectoryName);
            logger.Information("Everything is seems to be ok, starting obfuscation..");
            logger.Information(AsciiArt);

            var info = new IncompleteFileInfo(needs.FileName, needs.ReferencesDirectoryName, needs.OutputPath);
            var engine = new BitMonoStarter(serviceProvider);
            var succeed = await engine.StartAsync(info, CancellationToken);
            if (!succeed)
            {
                logger.Fatal("Engine has fatal issues, unable to continue!");
                statusCode = KnownReturnStatuses.Failure;
                return statusCode;
            }

            if (obfuscation.OpenFileDestinationInFileExplorer)
            {
                try
                {
                    Process.Start(needs.OutputPath);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "An error occured while opening the destination file in explorer!");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Obfuscation Canceled!");
            statusCode = KnownReturnStatuses.Failure;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something went wrong! " + ex);
            statusCode = KnownReturnStatuses.Failure;
        }

        Console.CancelKeyPress -= OnCancelKeyPress;
        if (needs?.Way == ObfuscationNeedsWay.Readline)
        {
            Console.WriteLine("Enter anything to exit!");
            Console.ReadLine();
        }
        return statusCode;
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        CancellationTokenSource.Cancel();
        e.Cancel = true;
    }
}