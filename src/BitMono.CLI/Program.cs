public class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            const string ProtectionsFileName = nameof(BitMono) + "." + nameof(BitMono.Protections) + ".dll";

            var neededForObfuscation = await specifyNeededForObfuscationAsync(args);

            Console.Clear();
            Console.WriteLine("File: {0}", neededForObfuscation.FileName);
            Console.WriteLine("Dependencies (libs): {0}", neededForObfuscation.DependenciesDirectoryPath);
            Console.WriteLine("Everything is seems to be good, starting obfuscation..");

            var domainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var protectionsFile = Path.Combine(domainBaseDirectory, ProtectionsFileName);
            Assembly.LoadFrom(protectionsFile);

            var outputDirectoryName = Path.Combine(neededForObfuscation.FileBaseDirectory, "output");
            Directory.CreateDirectory(neededForObfuscation.DependenciesDirectoryPath);
            Directory.CreateDirectory(outputDirectoryName);

            var serviceProvider = new BitMonoApplication().RegisterModule(new BitMonoModule(configureLogger =>
            {
                configureLogger.WriteTo.Async(configure => configure.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}"));
            })).Build();

            var obfuscationConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoObfuscationConfiguration>();
            var protectionsConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoProtectionsConfiguration>();
            var appSettingsConfiguration = serviceProvider.LifetimeScope.Resolve<IBitMonoAppSettingsConfiguration>();
            var obfuscationAttributeResolver = serviceProvider.LifetimeScope.Resolve<ObfuscationAttributeResolver>();
            var membersResolver = serviceProvider.LifetimeScope.Resolve<ICollection<IMemberResolver>>().ToList();
            var protections = serviceProvider.LifetimeScope.Resolve<ICollection<IProtection>>().ToList();
            var protectionSettings = protectionsConfiguration.GetProtectionSettings();
            var logger = serviceProvider.LifetimeScope.Resolve<ILogger>().ForContext<Program>();

            var moduleDefMDWriter = new CLIDataWriter();
            var dependenciesDataResolver = new DependenciesDataResolver(neededForObfuscation.DependenciesDirectoryPath);
            var bitMonoContext = new BitMonoContextCreator(dependenciesDataResolver, obfuscationConfiguration).Create(outputDirectoryName, neededForObfuscation.FileName);
            var cancellationTokenSource = new CancellationTokenSource();

            var engine = new BitMonoEngine(moduleDefMDWriter, obfuscationAttributeResolver, obfuscationConfiguration, membersResolver, protections, protectionSettings, logger);
            var succeed = await engine.StartAsync(bitMonoContext, bitMonoContext.FileName, cancellationTokenSource.Token);
            if (succeed == false)
            {
                logger.Fatal("Engine has issues, unable to continue, stop!");
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
    private static async Task<NeededForObfuscation> specifyNeededForObfuscationAsync(string[] args)
    {
        var fileName = await new CLIBitMonoModuleFileResolver(args).ResolveAsync();
        var specifyingFile = true;
        while (specifyingFile)
        {
            try
            {
                Console.WriteLine("Please, specify file or drag-and-drop in BitMono CLI");
                fileName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(fileName) == false)
                {
                    if (File.Exists(fileName))
                    {
                        specifyingFile = false;
                        Console.WriteLine("File succesfully specified: {0}", fileName);
                    }
                    else
                    {
                        Console.WriteLine("File cannot be found, please, try again!");
                    }
                }
                else
                {
                    Console.WriteLine("Unable to specify empty null or whitespace file, please, try again!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong while specifying the file: " + ex.ToString());
            }
        }

        var fileBaseDirectory = Path.GetDirectoryName(fileName);
        var dependenciesDirectoryName = Path.Combine(fileBaseDirectory, "libs");
        if (Directory.Exists(dependenciesDirectoryName) == false)
        {
            var specifyingDependencies = true;
            while (specifyingDependencies)
            {
                try
                {
                    Console.WriteLine("Please, specify dependencies (libs) path: ");
                    dependenciesDirectoryName = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(dependenciesDirectoryName) == false)
                    {
                        if (Directory.Exists(dependenciesDirectoryName))
                        {
                            Console.WriteLine("Dependencies (libs) succesfully specified: {0}!", dependenciesDirectoryName);
                            specifyingDependencies = false;
                        }
                        else
                        {
                            Console.WriteLine("Libs directory doesn't exist, please, try again!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unable to specify empty null or whitespace dependencies (libs), please, try again!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Something went wrong while specifying the dependencies (libs) path: " + ex.ToString());
                }
            }
        }
        else
        {
            Console.WriteLine("Dependencies (libs) directory was automatically found in: {0}!", dependenciesDirectoryName);
        }

        return new NeededForObfuscation
        {
            FileName = fileName,
            FileBaseDirectory = fileBaseDirectory,
            DependenciesDirectoryPath = dependenciesDirectoryName,
        };
    }
}