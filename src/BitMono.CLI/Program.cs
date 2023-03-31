#pragma warning disable CS8604
namespace BitMono.CLI;

internal class Program
{
    private static readonly string BitMonoFileVersionText =
        $"BitMono v{FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).FileVersion}";

    private static readonly string AsciiArt = @$"
       ___  _ __  __  ___
      / _ )(_) /_/  |/  /__  ___  ___
     / _  / / __/ /|_/ / _ \/ _ \/ _ \
    /____/_/\__/_/  /_/\___/_//_/\___/
    https://github.com/sunnamed434/BitMono
    {BitMonoFileVersionText}";

    private static async Task<int> Main(string[] args)
    {
        int errorCode = 0; //success
        try
        {
            Console.Title = BitMonoFileVersionText;
            var module = new BitMonoModule(
                configureContainer => configureContainer.AddProtections(),
                configureServices => configureServices.AddConfigurations(),
                configureLogger => configureLogger.WriteTo.AddConsoleLogger());

            await using var serviceProvider = new BitMonoApplication()
                .RegisterModule(module)
                .Build();

            var lifetimeScope = serviceProvider.LifetimeScope;
            var obfuscation = lifetimeScope.Resolve<IOptions<Shared.Models.Obfuscation>>().Value;
            var logger = lifetimeScope
                .Resolve<ILogger>()
                .ForContext<Program>();
            var needs = new ObfuscationNeedsFactory(args, logger).Create();
            if (needs == null)
            {
                errorCode = 1; //failure
                return errorCode;
            }

            Console.Clear();
            logger.Information("File: {0}", needs.FileName);
            logger.Information("Dependencies (libs): {0}", needs.ReferencesDirectoryName);
            logger.Information("Everything is seems to be ok, starting obfuscation..");
            logger.Information(AsciiArt);

            var cancellationTokenSource = new CancellationTokenSource();
            var engine = new BitMonoEngine(lifetimeScope);
            var succeed = await engine.StartAsync(needs, cancellationTokenSource.Token);
            if (succeed == false)
            {
                logger.Fatal("Engine has fatal issues, unable to continue!");
                Console.ReadLine();
                errorCode = 1; //failure
                return errorCode;
            }

            if (obfuscation.OpenFileDestinationInFileExplorer)
            {
                Process.Start(needs.OutputDirectoryName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something went wrong! " + ex);
            errorCode = 1; //failure
        }
        Console.WriteLine("Enter anything to exit!");
        Console.ReadLine();
        return errorCode;
    }
}
