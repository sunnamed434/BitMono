public class Program
{
    const string Protections = nameof(BitMono) + "." + nameof(BitMono.Protections) + ".dll";
    static CancellationTokenSource CancellationToken = new CancellationTokenSource();

    private static async Task Main(string[] args)
    {
        try
        {
            var moduleFileName = await new CLIBitMonoModuleFileResolver(args).ResolveAsync();
            if (string.IsNullOrWhiteSpace(moduleFileName))
            {
                while (string.IsNullOrWhiteSpace(moduleFileName))
                {
                    Console.Clear();
                    Console.WriteLine("Please, specify file or drag-and-drop in BitMono CLI");
                    moduleFileName = Console.ReadLine();
                }
            }
            var moduleFileBaseDirectory = Path.GetDirectoryName(moduleFileName);
            var dependenciesDirectoryName = Path.Combine(moduleFileBaseDirectory, "libs");
            if (Directory.Exists(dependenciesDirectoryName) == false)
            {
                while (string.IsNullOrWhiteSpace(dependenciesDirectoryName))
                {
                    Console.Clear();
                    Console.WriteLine("Please, specify dependencies (libs) path: ");
                    dependenciesDirectoryName = Console.ReadLine();
                }
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Dependencies (libs) directory was automatically found in: {0}!", dependenciesDirectoryName);
            }

            var domainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var protectionsFile = Path.Combine(domainBaseDirectory, Protections);
            Assembly.LoadFrom(protectionsFile);

            var outputDirectoryName = Path.Combine(moduleFileBaseDirectory, "output");
            Directory.CreateDirectory(dependenciesDirectoryName);
            Directory.CreateDirectory(outputDirectoryName);

            var serviceProvider = new BitMonoApplication().RegisterModule(new BitMonoModule(configureLogger =>
            {
                configureLogger.WriteTo.Async(configure => configure.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}"));
            })).Build();

            var obfuscationConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoObfuscationConfiguration>();
            var protectionsConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoProtectionsConfiguration>();
            var appSettingsConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoAppSettingsConfiguration>();
            var obfuscationAttributeResolver = serviceProvider.LifetimeScope.Resolve<IObfuscationAttributeResolver>();
            var memberDefinitionResolver = serviceProvider.LifetimeScope.Resolve<ICollection<IMemberDefinitionfResolver>>().ToList();
            var protections = serviceProvider.LifetimeScope.Resolve<ICollection<IProtection>>().ToList();
            var protectionSettings = protectionsConfiguration.GetProtectionSettings();
            var logger = serviceProvider.LifetimeScope.Resolve<ILogger>().ForContext<Program>();

            var moduleDefMDWriter = new CLIDataWriter();
            var dependenciesDataResolver = new DependenciesDataResolver(dependenciesDirectoryName);
            var bitMonoContext = new BitMonoContextCreator(dependenciesDataResolver, obfuscationConfiguration).Create(outputDirectoryName, moduleFileName);

            await new BitMonoEngine(
                moduleDefMDWriter,
                obfuscationAttributeResolver,
                obfuscationConfiguration,
                memberDefinitionResolver,
                protections,
                protectionSettings,
                logger)
                .ObfuscateAsync(bitMonoContext, bitMonoContext.FileName, CancellationToken);

            if (CancellationToken.IsCancellationRequested)
            {
                logger.Fatal("Operation cancelled!");
                Console.ReadLine();
                return;
            }
            if (obfuscationConfiguration.Configuration.GetValue<bool>(nameof(Obfuscation.OpenFileDestinationInFileExplorer)))
            {
                Process.Start(bitMonoContext.OutputDirectoryName);
            }

            new TipsNotifier(appSettingsConfiguration, logger).Notify();
            await serviceProvider.DisposeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Something went wrong! " + ex.ToString());
        }
        Console.ReadLine();
    }
}