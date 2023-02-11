#pragma warning disable CS8604
namespace BitMono.CLI;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            var needs = new CLIObfuscationNeedsFactory(args).Create();
            Console.Clear();
            Console.WriteLine("File: {0}", needs.FileName);
            Console.WriteLine("Dependencies (libs): {0}", needs.DependenciesDirectoryName);
            Console.WriteLine("Everything is seems to be good, starting obfuscation..");

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