namespace BitMono.CLI.Modules;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CLIObfuscationNeedsFactory : IObfuscationNeedsFactory
{
    private readonly string[] m_Args;

    public CLIObfuscationNeedsFactory(string[] args)
    {
        m_Args = args;
    }

    public ObfuscationNeeds Create()
    {
        var fileName = CLIBitMonoModuleFileResolver.Resolve(m_Args);
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
                        Console.WriteLine("File successfully specified: {0}", fileName);
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
                            Console.WriteLine("Dependencies (libs) successfully specified: {0}!", dependenciesDirectoryName);
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

        var outputDirectoryName = Path.Combine(fileBaseDirectory, "output");
        Directory.CreateDirectory(outputDirectoryName);
        Directory.CreateDirectory(dependenciesDirectoryName);

        return new ObfuscationNeeds
        {
            FileName = fileName,
            FileBaseDirectory = fileBaseDirectory,
            DependenciesDirectoryName = dependenciesDirectoryName,
            OutputDirectoryName = outputDirectoryName
        };
    }
}