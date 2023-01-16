namespace BitMono.CLI;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            const string ProtectionsFileName = nameof(BitMono) + "." + nameof(BitMono.Protections) + ".dll";
 
            var needs = new CLIObfuscationNeedsFactory(args).Create();
            Console.Clear();
            Console.WriteLine("File: {0}", needs.FileName);
            Console.WriteLine("Dependencies (libs): {0}", needs.DependenciesDirectoryName);
            Console.WriteLine("Everything is seems to be good, starting obfuscation..");

            var domainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var protectionsFile = Path.Combine(domainBaseDirectory, ProtectionsFileName);
            Assembly.LoadFrom(protectionsFile);
            
            var serviceProvider = new BitMonoApplication().RegisterModule(new BitMonoModule(configureLogger: configureLogger =>
            {
                configureLogger.WriteTo.Async(configure => configure.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}"));
            })).Build();

            var obfuscation = serviceProvider.LifetimeScope.Resolve<IOptions<Shared.Models.Obfuscation>>().Value;
            var protectionSettings = serviceProvider.LifetimeScope.Resolve<IOptions<ProtectionSettings>>().Value;
            var obfuscationAttributeResolver = serviceProvider.LifetimeScope.Resolve<ObfuscationAttributeResolver>();
            var membersResolver = serviceProvider.LifetimeScope.Resolve<ICollection<IMemberResolver>>().ToList();
            var protections = serviceProvider.LifetimeScope.Resolve<ICollection<IProtection>>().ToList();
            var logger = serviceProvider.LifetimeScope.Resolve<ILogger>().ForContext<Program>();

            var cancellationTokenSource = new CancellationTokenSource();

            var engine = new BitMonoEngine(obfuscationAttributeResolver, obfuscation, protectionSettings, membersResolver, protections, logger);
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

            await serviceProvider.DisposeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something went wrong! " + ex);
        }
        Console.WriteLine("Press any key to exit!");
        Console.ReadLine();
    }
}