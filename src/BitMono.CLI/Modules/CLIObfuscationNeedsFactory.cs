namespace BitMono.CLI.Modules;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CLIObfuscationNeedsFactory : IObfuscationNeedsFactory
{
    private readonly string[] m_Args;
    private readonly ILogger m_Logger;

    public CLIObfuscationNeedsFactory(string[] args, ILogger logger)
    {
        m_Args = args;
        m_Logger = logger.ForContext<CLIObfuscationNeedsFactory>();
    }

    public ObfuscationNeeds Create()
    {
        var fileName = CLIBitMonoModuleFileResolver.Resolve(m_Args);
        var specifyingFile = true;
        while (specifyingFile)
        {
            try
            {
                m_Logger.Information("Please, specify file or drag-and-drop in BitMono CLI");
                fileName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(fileName) == false)
                {
                    if (File.Exists(fileName))
                    {
                        specifyingFile = false;
                        m_Logger.Information("File successfully specified: {0}", fileName);
                    }
                    else
                    {
                        m_Logger.Warning("File cannot be found, please, try again!");
                    }
                }
                else
                {
                    m_Logger.Warning("Unable to specify empty null or whitespace file, please, try again!");
                }
            }
            catch (Exception ex)
            {
                m_Logger.Warning("Something went wrong while specifying the file: " + ex);
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
                    if (Directory.Exists(dependenciesDirectoryName))
                    {
                        m_Logger.Information("Dependencies (libs) successfully found automatically: {0}!", dependenciesDirectoryName);
                        specifyingDependencies = false;
                        break;
                    }

                    m_Logger.Information("Please, specify dependencies (libs) path: ");
                    var newDependenciesDirectoryName = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newDependenciesDirectoryName) == false)
                    {
                        if (Directory.Exists(newDependenciesDirectoryName))
                        {
                            dependenciesDirectoryName = newDependenciesDirectoryName;
                            m_Logger.Information("Dependencies (libs) successfully specified: {0}!", newDependenciesDirectoryName);
                            specifyingDependencies = false;
                        }
                        else
                        {
                            m_Logger.Information("Libs directory doesn't exist, please, try again!");
                        }
                    }
                    else
                    {
                        m_Logger.Information("Unable to specify empty (libs), please, try again!");
                    }
                }
                catch (Exception ex)
                {
                    m_Logger.Information("Something went wrong while specifying the dependencies (libs) path: " + ex);
                }
            }
        }
        else
        {
            m_Logger.Information("Dependencies (libs) directory was automatically found in: {0}!", dependenciesDirectoryName);
        }

        var outputDirectoryName = Path.Combine(fileBaseDirectory, "output");
        Directory.CreateDirectory(outputDirectoryName);
        Directory.CreateDirectory(dependenciesDirectoryName);
        return new ObfuscationNeeds
        {
            FileName = fileName,
            FileBaseDirectory = fileBaseDirectory,
            ReferencesDirectoryName = dependenciesDirectoryName,
            OutputDirectoryName = outputDirectoryName
        };
    }
}