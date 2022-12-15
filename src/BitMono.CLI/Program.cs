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
            var runtimeModuleDefMD = ModuleDefMD.Load(typeof(BitMono.Runtime.Hooking).Module);
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
            var dnlibDefFeatureObfuscationAttributeHavingResolver = serviceProvider.LifetimeScope.Resolve<IDnlibDefObfuscationAttributeResolver>();

            var bitMonoContext = new BitMonoContextCreator(new DependenciesDataResolver(dependenciesDirectoryName), obfuscationConfiguration).Create(outputDirectoryName);
            bitMonoContext.ModuleFileName = moduleFileName;

            var moduleDefMDWriter = new CLIModuleDefMDWriter();
            var moduleFileBytes = File.ReadAllBytes(bitMonoContext.ModuleFileName);
            var moduleDefMDCreator = new ModuleCreator(moduleFileBytes);
            var dnlibDefResolvers = serviceProvider.LifetimeScope.Resolve<ICollection<IDnlibDefResolver>>().ToList();
            var protections = serviceProvider.LifetimeScope.Resolve<ICollection<IProtection>>().ToList();
            var protectionSettings = protectionsConfiguration.GetProtectionSettings();
            var logger = serviceProvider.LifetimeScope.Resolve<ILogger>().ForContext<Program>();

            var moduleDefMDCreationResult = moduleDefMDCreator.Create();
            var protectionContext = new ProtectionContextCreator(moduleDefMDCreationResult, runtimeModuleDefMD, bitMonoContext).Create();

            Console.ReadLine();
            return;
            await new BitMonoEngine(
                moduleDefMDWriter,
                moduleDefMDCreator,
                dnlibDefFeatureObfuscationAttributeHavingResolver,
                obfuscationConfiguration,
                dnlibDefResolvers,
                protections,
                protectionSettings,
                logger)
                .ObfuscateAsync(bitMonoContext, runtimeModuleDefMD, CancellationToken);

            if (CancellationToken.IsCancellationRequested)
            {
                logger.Fatal("Operation cancelled!");
                Console.ReadLine();
                return;
            }

            if (obfuscationConfiguration.Configuration.GetValue<bool>(nameof(Obfuscation.OpenFileDestinationInFileExplorer)))
            {
                Process.Start(bitMonoContext.OutputPath);
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