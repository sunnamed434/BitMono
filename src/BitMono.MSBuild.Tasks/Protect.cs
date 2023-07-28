using BitMono.Obfuscation.Files;
using BitMono.Obfuscation.Starter;

namespace BitMono.MSBuild.Tasks;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class Protect : Task
{
    public ITaskItem TaskAssembly { get; set; }
    public ITaskItem[] References { get; set; }
    [Required]
    public ITaskItem AssemblyPath { get; set; }
    public bool BitMonoEnabled { get; set; }
    public ITaskItem BitMonoProtections { get; set; }
    public ITaskItem BitMonoCriticals { get; set; }
    public ITaskItem BitMonoObfuscation { get; set; }

    public override bool Execute()
    {
        var path = Path.GetFullPath(TaskAssembly.ItemSpec);
        var path2 = Path.GetDirectoryName(TaskAssembly.ItemSpec);
        Log.LogMessage(MessageImportance.High, "path: " + path);
        Log.LogMessage(MessageImportance.High, "path2: " + path2);
        Log.LogMessage(MessageImportance.High, "TaskAssembly: " + TaskAssembly.ItemSpec);
        Log.LogMessage(MessageImportance.Normal, "HELLO WORLD - NORMAL");
        Log.LogMessage(MessageImportance.High, "HELLO WORLD - HIGH");
        Log.LogMessage(MessageImportance.High, "References: " + (References != null ? References.Length : "References NULL") );
        Log.LogMessage(MessageImportance.High, "BitMonoEnabled: " + BitMonoEnabled);
        Log.LogMessage(MessageImportance.High, "AssemblyPath: " + (AssemblyPath != null ? AssemblyPath : "AssemblyPath NULL"));
        Log.LogMessage(MessageImportance.High, "BitMonoProtections: " + (BitMonoProtections != null ? BitMonoProtections : "BitMonoProtections NULL"));
        Log.LogMessage(MessageImportance.High, "BitMonoCriticals: " + (BitMonoCriticals != null ? BitMonoCriticals : "BitMonoCriticals NULL"));
        Log.LogMessage(MessageImportance.High, "BitMonoObfuscation: " + (BitMonoObfuscation != null ? BitMonoObfuscation : "BitMonoObfuscation NULL"));

        var module = new BitMonoModule(
            configureServices => configureServices.AddProtections(path2),
            configureServices => configureServices.AddConfigurations(BitMonoProtections.ItemSpec, BitMonoCriticals.ItemSpec, BitMonoObfuscation.ItemSpec),
            configureLogger => configureLogger.WriteTo.MSBuild(this));
        using var serviceProvider = new BitMonoApplication()
            .RegisterModule(module)
            .Build();

        var logger = serviceProvider.LifetimeScope
            .Resolve<ILogger>()
            .ForContext<Protect>();

        logger.Fatal("THIS IS WORKING!!!!");

        logger.Information("HELLO WORLD #2!");
        logger.Information("PATH: " + AssemblyPath.ItemSpec);
        var engine = new BitMonoStarter(serviceProvider);
        var fileName = AssemblyPath.ItemSpec;
        var fileBaseDirectory = Path.GetDirectoryName(Path.GetFullPath(fileName));
        var referencesDirectoryName = fileBaseDirectory;
        var info = new IncompleteFileInfo(AssemblyPath.ItemSpec, referencesDirectoryName, fileBaseDirectory);

        engine.StartAsync(info, CancellationToken.None).GetAwaiter();
        logger.Information("THIS IS DONE!");

        //engine.StartAsync(needs, );
        return true;
    }
}