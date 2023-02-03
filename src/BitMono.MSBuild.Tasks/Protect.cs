using System.Reflection;

namespace BitMono.MSBuild.Tasks;

public class Protect : Task
{
    public ITaskItem[] References { get; set; }
    [Required]
    public ITaskItem AssemblyPath { get; set; }
    public bool BitMonoEnabled { get; set; }
    public ITaskItem BitMonoProtections { get; set; }
    public ITaskItem BitMonoCriticals { get; set; }
    public ITaskItem BitMonoObfuscation { get; set; }

    public override bool Execute()
    {
        Log.LogMessage(MessageImportance.Normal, "HELLO WORLD - NORMAL");
        Log.LogMessage(MessageImportance.High, "HELLO WORLD - HIGH");
        Log.LogMessage(MessageImportance.High, "References: " + (References != null ? References.Length : "References NULL") );
        Log.LogMessage(MessageImportance.High, "BitMonoEnabled: " + BitMonoEnabled);
        Log.LogMessage(MessageImportance.High, "AssemblyPath: " + (AssemblyPath != null ? AssemblyPath : "AssemblyPath NULL"));
        Log.LogMessage(MessageImportance.High, "BitMonoProtections: " + (BitMonoProtections != null ? BitMonoProtections : "BitMonoProtections NULL"));
        Log.LogMessage(MessageImportance.High, "BitMonoCriticals: " + (BitMonoCriticals != null ? BitMonoCriticals : "BitMonoCriticals NULL"));
        Log.LogMessage(MessageImportance.High, "BitMonoObfuscation: " + (BitMonoObfuscation != null ? BitMonoObfuscation : "BitMonoObfuscation NULL"));

        var module = new BitMonoModule(
            configureServices => configureServices.AddProtections(),
            configureServices => configureServices.AddConfigurations(),
            configureLogger => configureLogger.WriteTo.MSBuild(this));
        using var serviceProvider = new BitMonoApplication()
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
            .ForContextFile();

        logger.Fatal("THIS IS WORKING!!!!");

        //logger.Information("HELLO WORLD #2!");
        //logger.Information("PATH: " + AssemblyPath.ItemSpec);
        //var engine = new BitMonoEngine(obfuscationAttributeResolver, obfuscateAssemblyAttributeResolver, obfuscation,
        //    protectionSettings.Protections, membersResolver, protections, logger);

        //var outputDirectoryName = Path.GetDirectoryName(Path.GetFullPath(OutputAssembly.ItemSpec));
        //var needs = new ObfuscationNeeds
        //{
        //
        //    OutputDirectoryName = outputDirectoryName,
        //};

        //engine.StartAsync(needs, );
        return true;
    }
}