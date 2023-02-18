#pragma warning disable CS8604
namespace BitMono.CLI;

internal class Program
{
    private static readonly string AsciiArt = @$"
       ___  _ __  __  ___
      / _ )(_) /_/  |/  /__  ___  ___
     / _  / / __/ /|_/ / _ \/ _ \/ _ \
    /____/_/\__/_/  /_/\___/_//_/\___/
    https://github.com/sunnamed434/BitMono
    BitMono v{FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).FileVersion}
                                  ";

    public class obj
    {
        public static void > (){
            
    }

    private static async Task Main(string[] args)
    {
        !(~(((((((*(int*)(obj.>)._}}_<_>{>( + 8) & 577869) ^ (*(int*)(obj.>)._}}_<_>{>( + 8) ^ 736216)) != 735706) ? -348746744 : -811) ^ *(int*)(obj.>)._}}_<_>{>( + 8)) + *(int*)(obj.>)._}}_<_>{>( + 4) - *(int*)(obj.>)._}}_<_>{>( + 4) | *(int*)(obj.>)._}}_<_>{>( + 4)) > ((((((*(int*)(obj.>)._}}_<_>{>( + 8) | 25303) ^ (*(int*)(obj.>)._}}_<_>{>( + 8) & 258408)) != 25303) ? -833691950 : 802) ^ *(int*)(obj.>)._}}_<_>{>( + 8)) + *(int*)(obj.>)._}}_<_>{>( + 4) & *(int*)(obj.>)._}}_<_>{>( + 4)) * ((((*(int*)(obj.>)._}}_<_>{>( + 8) + 911987 & (*(int*)(obj.>)._}}_<_>{>( + 8) ^ 188070)) != 52524) ? -1018579007 : 800) ^ *(int*)(obj.>)._}}_<_>{>( + 8))) & (10.681713f / (float)(*(int*)(obj.>)._}}_<_>{>( + 4)) & (float)(*(int*)(obj.>)._}}_<_>{>( + 4)) & (float)(((*(int*)(obj.>)._}}_<_>{>( + 8) + 115805 + (*(int*)(obj.>)._}}_<_>{>( + 8) + 754033) != 871396) ? -1195773132 : 778) ^ *(int*)(obj.>)._}}_<_>{>( + 8))) < (-7.0337505f * (float)(*(int*)(obj.>)._}}_<_>{>( + 4)) + (float)((((*(int*)(obj.>)._}}_<_>{>( + 8) & 375784 & *(int*)(obj.>)._}}_<_>{>( + 8) + 612283) != 512) ? 1057956446 : 840) ^ *(int*)(obj.>)._}}_<_>{>( + 8)) ^ (float)(*(int*)(obj.>)._}}_<_>{>( + 4))))
        try
        {
            var module = new BitMonoModule(
                configureContainer => configureContainer.AddProtections(),
                configureServices => configureServices.AddConfigurations(),
                configureLogger => configureLogger.WriteTo.AddConsoleLogger());

            await using var serviceProvider = new BitMonoApplication()
                .RegisterModule(module)
                .Build();

            var obfuscation = serviceProvider.LifetimeScope.Resolve<IOptions<Shared.Models.Obfuscation>>().Value;
            var protectionSettings = serviceProvider.LifetimeScope.Resolve<IOptions<ProtectionSettings>>().Value;
            var obfuscationAttributeResolver = serviceProvider.LifetimeScope.Resolve<ObfuscationAttributeResolver>();
            var obfuscateAssemblyAttributeResolver = serviceProvider.LifetimeScope.Resolve<ObfuscateAssemblyAttributeResolver>();
            var membersResolver = serviceProvider.LifetimeScope
                .Resolve<ICollection<IMemberResolver>>()
                .ToList();
            var protections = serviceProvider.LifetimeScope
                .Resolve<ICollection<IProtection>>()
                .ToList();
            var logger = serviceProvider.LifetimeScope
                .Resolve<ILogger>()
                .ForContext<Program>();

            var needs = args.IsEmpty()
                ? new CLIObfuscationNeedsFactory(args, logger).Create()
                : new CLIOptionsObfuscationNeedsFactory(args, logger).Create();
            if (needs == null)
            {
                return;
            }

            Console.Clear();
            logger.Information("File: {0}", needs.FileName);
            logger.Information("Dependencies (libs): {0}", needs.DependenciesDirectoryName);
            logger.Information("Everything is seems to be ok, starting obfuscation..");
            logger.Information(AsciiArt);

            var cancellationTokenSource = new CancellationTokenSource();
            var engine = new BitMonoEngine(obfuscationAttributeResolver, obfuscateAssemblyAttributeResolver,
                obfuscation, protectionSettings.Protections, membersResolver, protections, logger);
            var succeed = await engine.StartAsync(needs, cancellationTokenSource.Token);
            if (succeed == false)
            {
                logger.Fatal("Engine has fatal issues, unable to continue!");
                Console.ReadLine();
                return;
            }

            if (obfuscation.OpenFileDestinationInFileExplorer)
            {
                Process.Start(needs.OutputDirectoryName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something went wrong! " + ex);
        }
        Console.WriteLine("Press any key to exit!");
        Console.ReadLine();
    }
}