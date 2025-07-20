namespace BitMono.CLI.Modules;

internal class ReadlineObfuscationNeedsFactory
{
    private readonly string[] _args;
    private readonly ObfuscationSettings _obfuscationSettings;
    private readonly List<ProtectionSetting> _protectionSettings;
    private readonly ILogger _logger;

    public ReadlineObfuscationNeedsFactory(string[] args, ObfuscationSettings obfuscationSettings,
        List<ProtectionSetting> protectionSettings, ILogger logger)
    {
        _args = args;
        _obfuscationSettings = obfuscationSettings;
        _protectionSettings = protectionSettings;
        _logger = logger.ForContext<ReadlineObfuscationNeedsFactory>();
    }

    public ObfuscationNeeds Create(CancellationToken cancellationToken)
    {
        var fileName = GetFileName(_args);
        while (true)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.Information("Please, specify file or drag-and-drop in BitMono CLI");

                fileName = PathFormatterUtility.Format(Console.ReadLine());
                cancellationToken.ThrowIfCancellationRequested();
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    if (File.Exists(fileName))
                    {
                        _logger.Information("File successfully specified: {0}", fileName);
                        break;
                    }

                    _logger.Warning("File cannot be found, please, try again!");
                }
                else
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _logger.Warning("Unable to specify empty null or whitespace file, please, try again!");
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Something went wrong while specifying the file");
            }
        }

        List<string> protections = [];

        bool hasEnabledProtections = _protectionSettings != null && _protectionSettings.Any(x => x.Enabled);
        bool hasAnyProtectionSettings = _protectionSettings != null && _protectionSettings.Any();

        if (!hasEnabledProtections)
        {
            if (!hasAnyProtectionSettings)
            {
                _logger.Warning("No protection settings found (protections.json may be missing or empty)");
                _logger.Information("Please input the preferred protections with ',' delimiter, example: StringsEncryption,AntiDe4dot,ControlFlow");
            }
            else
            {
                _logger.Warning("No protection is enabled in protections.json file, please either enable any protection first or input the preferred with ',' delimiter, example: StringsEncryption,AntiDe4dot");
            }

            while (true)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var protectionInput = Console.ReadLine();
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!string.IsNullOrWhiteSpace(protectionInput))
                    {
                        var inputProtections = protectionInput.Split([','], StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => p.Trim())
                            .Where(p => !string.IsNullOrWhiteSpace(p))
                            .ToList();

                        if (inputProtections.Any())
                        {
                            // If we have protection settings, validate against them but allow unknown protections
                            if (hasAnyProtectionSettings)
                            {
                                var availableProtections = _protectionSettings.Select(p => p.Name).ToList();
                                var unknownProtections = inputProtections.Where(p => !availableProtections.Contains(p, StringComparer.OrdinalIgnoreCase)).ToList();

                                if (unknownProtections.Any())
                                {
                                    _logger.Warning("The following protection(s) are not found in current protections.json but will be used anyway: {0}",
                                        string.Join(", ", unknownProtections));
                                    _logger.Information("Available protections in config: {0}",
                                        availableProtections.Any() ? string.Join(", ", availableProtections) : "none");
                                }

                                var knownProtections = inputProtections.Where(p => availableProtections.Contains(p, StringComparer.OrdinalIgnoreCase)).ToList();
                                if (knownProtections.Any())
                                {
                                    _logger.Information("Recognized protection(s) from config: {0}", string.Join(", ", knownProtections));
                                }
                            }
                            else
                            {
                                _logger.Warning("Cannot validate protection names as protections.json is missing/empty. Using specified protections as-is.");
                            }

                            protections = inputProtections;
                            _logger.Information("Protections successfully specified: {0}", string.Join(", ", protections));
                            break;
                        }

                        _logger.Warning("No valid protections found in input, please try again!");
                    }
                    else
                    {
                        _logger.Information("No protections specified, continuing without additional protections...");
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Something went wrong while specifying protections");
                }
            }
        }
        else
        {
            // Use enabled protections from settings
            protections = _protectionSettings.Where(x => x.Enabled).Select(x => x.Name).ToList();
            _logger.Information("Using enabled protections from settings: {0}", string.Join(", ", protections));
        }

        string dependenciesDirectoryName;
        string outputDirectoryName;
        var fileBaseDirectory = Path.GetDirectoryName(fileName);
        if (_obfuscationSettings.ForceObfuscation)
        {
            dependenciesDirectoryName = fileBaseDirectory;
            outputDirectoryName = fileBaseDirectory;
        }
        else
        {
            outputDirectoryName = Path.Combine(fileBaseDirectory, _obfuscationSettings.OutputDirectoryName);
            dependenciesDirectoryName = Path.Combine(fileBaseDirectory, _obfuscationSettings.ReferencesDirectoryName);
            if (!Directory.Exists(dependenciesDirectoryName))
            {
                while (true)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (Directory.Exists(dependenciesDirectoryName))
                        {
                            _logger.Information("Dependencies (libs) successfully found automatically: {0}!",
                                dependenciesDirectoryName);
                            break;
                        }

                        _logger.Information("Please, specify dependencies (libs) path: ");
                        var newDependenciesDirectoryName = PathFormatterUtility.Format(Console.ReadLine());
                        if (!string.IsNullOrWhiteSpace(newDependenciesDirectoryName))
                        {
                            if (Directory.Exists(newDependenciesDirectoryName))
                            {
                                dependenciesDirectoryName = newDependenciesDirectoryName;
                                _logger.Information("Dependencies (libs) successfully specified: {0}!",
                                    newDependenciesDirectoryName);
                                break;
                            }
                            else
                            {
                                _logger.Information("Libs directory doesn't exist, please, try again!");
                            }
                        }
                        else
                        {
                            _logger.Information("Unable to specify empty (libs), please, try again!");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Something went wrong while specifying the dependencies (libs) path");
                    }
                }
            }
            else
            {
                _logger.Information("Dependencies (libs) directory was automatically found in: {0}",
                    dependenciesDirectoryName);
            }
        }

        Directory.CreateDirectory(outputDirectoryName);
        Directory.CreateDirectory(dependenciesDirectoryName);
        return new ObfuscationNeeds
        {
            FileName = fileName,
            FileBaseDirectory = fileBaseDirectory,
            ReferencesDirectoryName = dependenciesDirectoryName,
            OutputPath = outputDirectoryName,
            Way = ObfuscationNeedsWay.Readline,
            Protections = protections,
        };
    }

    private string GetFileName(string[] args)
    {
        string? file = null;
        if (!args.IsEmpty())
        {
            file = PathFormatterUtility.Format(args[0]);
        }
        return file;
    }
}