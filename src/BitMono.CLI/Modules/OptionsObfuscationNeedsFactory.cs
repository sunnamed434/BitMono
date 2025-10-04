namespace BitMono.CLI.Modules;

internal class OptionsObfuscationNeedsFactory
{
    private readonly string[] _args;

    public OptionsObfuscationNeedsFactory(string[] args)
    {
        _args = args;
    }

    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public ObfuscationNeeds? Create(CancellationToken cancellationToken)
    {
        var parser = new Parser(with =>
        {
            with.EnableDashDash = true;
            with.HelpWriter = Console.Error;
        });
        var parserResult = parser.ParseArguments<Options>(_args);
        if (!parserResult.Errors.IsEmpty())
        {
            return null;
        }
        var options = parserResult.Value;

        ObfuscationSettings? obfuscationSettings = null;
        try
        {
            if (options.ObfuscationFile != null && File.Exists(options.ObfuscationFile))
            {
                var obfuscationConfig = new BitMonoObfuscationConfiguration(options.ObfuscationFile);
                obfuscationSettings = obfuscationConfig.Configuration.Get<ObfuscationSettings>();
            }
            else if (File.Exists(KnownConfigNames.Obfuscation))
            {
                var obfuscationConfig = new BitMonoObfuscationConfiguration();
                obfuscationSettings = obfuscationConfig.Configuration.Get<ObfuscationSettings>();
            }

            if (obfuscationSettings != null && options.NoWatermark)
            {
                obfuscationSettings.Watermark = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load obfuscation configuration: {ex}");
        }

        var filePath = PathFormatterUtility.Format(options.File!);
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File {filePath} cannot be found, please, try again!");
            return null;
        }
        var fileBaseDirectory = Path.GetDirectoryName(filePath);

        ProtectionSettings? protectionSettings = null;
        if (options.Protections.Any())
        {
            protectionSettings = new ProtectionSettings
            {
                Protections = options.Protections.Select(x => new ProtectionSetting
                {
                    Name = x,
                    Enabled = true
                }).ToList()
            };
        }

        ObfuscationNeeds needs;
        if (obfuscationSettings?.ForceObfuscation == true)
        {
            needs = new ObfuscationNeeds
            {
                FileName = filePath,
                FileBaseDirectory = fileBaseDirectory,
                ReferencesDirectoryName = fileBaseDirectory,
                OutputPath = fileBaseDirectory,
                Protections = options.Protections.ToList(),
                ProtectionSettings = protectionSettings,
                Way = ObfuscationNeedsWay.Options,
                CriticalsFile = options.CriticalsFile,
                LoggingFile = options.LoggingFile,
                ObfuscationFile = options.ObfuscationFile,
                ProtectionsFile = options.ProtectionsFile,
                ObfuscationSettings = obfuscationSettings
            };
        }
        else
        {
            needs = new ObfuscationNeeds
            {
                FileName = filePath,
                FileBaseDirectory = fileBaseDirectory,
                ReferencesDirectoryName = options.Libraries?.IsNullOrEmpty() == false
                    ? options.Libraries
                    : Path.Combine(fileBaseDirectory, obfuscationSettings?.ReferencesDirectoryName ?? "libs"),
                OutputPath = options.Output?.IsNullOrEmpty() == false
                    ? options.Output
                    : Path.Combine(fileBaseDirectory, obfuscationSettings?.OutputDirectoryName ?? "output"),
                Protections = options.Protections.ToList(),
                ProtectionSettings = protectionSettings,
                Way = ObfuscationNeedsWay.Options,
                CriticalsFile = options.CriticalsFile,
                LoggingFile = options.LoggingFile,
                ObfuscationFile = options.ObfuscationFile,
                ProtectionsFile = options.ProtectionsFile,
                ObfuscationSettings = obfuscationSettings
            };
        }

        Directory.CreateDirectory(needs.OutputPath);
        Directory.CreateDirectory(needs.ReferencesDirectoryName);
        return needs;
    }
}