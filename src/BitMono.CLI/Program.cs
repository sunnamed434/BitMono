namespace BitMono.CLI;

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
        try
        {
            Console.Title = BitMonoFileVersionText;
            var module = new BitMonoModule(
                configureContainer => configureContainer.AddProtections(),
                configureServices => configureServices.AddConfigurations(),
                configureLogger => configureLogger.WriteTo.AddConsoleLogger());

            var app = new BitMonoApplication().RegisterModule(module);
            await using var serviceProvider = await app.BuildAsync(CancellationToken);

            var obfuscation = serviceProvider.GetRequiredService<IOptions<ObfuscationSettings>>().Value;
            var logger = serviceProvider
                .GetRequiredService<ILogger>()
                .ForContext<Program>();
            var needs = new ObfuscationNeedsFactory(args, obfuscation, logger).Create(CancellationToken);
            if (needs == null)
            {
                statusCode = KnownReturnStatuses.Failure;
                return statusCode;
            }

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
            if (succeed == false)
            {
                logger.Fatal("Engine has fatal issues, unable to continue!");
                Console.ReadLine();
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
            statusCode = KnownReturnStatuses.Cancel;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something went wrong! " + ex);
            statusCode = KnownReturnStatuses.Failure;
        }

        Console.CancelKeyPress -= OnCancelKeyPress;

        Console.WriteLine("Enter anything to exit!");
        Console.ReadLine();
        return statusCode;
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        CancellationTokenSource.Cancel();
        e.Cancel = true;
    }
}