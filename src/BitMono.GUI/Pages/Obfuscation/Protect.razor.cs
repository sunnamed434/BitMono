using BitMono.Obfuscation.Abstractions;
using BitMono.Obfuscation.Factories;

namespace BitMono.GUI.Pages.Obfuscation;

public partial class Protect
{
    private string _dependenciesDirectoryName;
    private string _outputDirectoryName;
    private IBrowserFile _obfuscationFile;

    [Inject] public ILogger Logger { get; set; }
    [Inject] public ICollection<IMemberResolver> MemberResolvers { get; set; }
    [Inject] public ICollection<IProtection> Protections { get; set; }
    [Inject] public IStoringProtections StoringProtections { get; set; }
    [Inject] public IServiceProvider ServiceProvider { get; set; }
    public Highlight Highlight { get; set; }
    public bool ObfuscationInProcess { get; set; }

    protected override Task OnInitializedAsync()
    {
        HandlerLogEventSink.OnEnqueued += onEnqueuedHandleAsync;
        return Task.CompletedTask;
    }

    const string ExternalComponentsFile = nameof(BitMono) + "." + nameof(Runtime) + ".dll";
    public async Task ObfuscateAsync()
    {
        if (ObfuscationInProcess == false)
        {
            try
            {
                ObfuscationInProcess = true;
                var memoryStream = new MemoryStream();
                await _obfuscationFile.OpenReadStream().CopyToAsync(memoryStream);
                var moduleBytes = memoryStream.ToArray();

                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var runtimeModule = ModuleDefinition.FromFile(Path.Combine(baseDirectory, ExternalComponentsFile));

                var obfuscation = ServiceProvider.GetRequiredService<IOptions<BitMono.Shared.Models.Obfuscation>>().Value;
                var obfuscationAttributeResolver = ServiceProvider.GetRequiredService<ObfuscationAttributeResolver>();

                var dependencies = Directory.GetFiles(_dependenciesDirectoryName);
                var dependeciesData = new List<byte[]>();
                for (var i = 0; i < dependencies.Length; i++)
                {
                    dependeciesData.Add(File.ReadAllBytes(dependencies[i]));
                }

                var dataResolver = new ReferencesDataResolver(_dependenciesDirectoryName);
                //var bitMonoContextFactory = new BitMonoContextFactory(dataResolver, obfuscation);
                //var bitMonoContext = bitMonoContextFactory.Create(_outputDirectoryName, _obfuscationFile.Name);
                //var engine = new BitMonoEngine(obfuscationAttributeResolver, obfuscation, StoringProtections.Protections, MemberResolvers.ToList(),  Protections.ToList(), Logger);
                //await engine.StartAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occured while obfuscating!");
            }
            finally
            {
                ObfuscationInProcess = false;
            }
        }
    }
    public async Task ObfuscateFileAsync()
    {
        await hideObfuscationInfoAlert();

        await Highlight.FlushAsync();
        if (_obfuscationFile == null)
        {
            await AlertsContainer.ShowAlertAsync("obfuscation-info", "Please, specify file to be protected!", Alerts.Danger);
            return;
        }
        if (string.IsNullOrWhiteSpace(_dependenciesDirectoryName))
        {
            await AlertsContainer.ShowAlertAsync("obfuscation-info", "Please, specify dependencies folder!", Alerts.Danger);
            return;
        }
        if (string.IsNullOrWhiteSpace(_outputDirectoryName))
        {
            await AlertsContainer.ShowAlertAsync("obfuscation-info", "Please, specify output folder!", Alerts.Danger);
            return;
        }

        await ObfuscateAsync();
        await AlertsContainer.ShowAlertAsync("obfuscation-info", "Protected!", Alerts.Success);
    }
    public async Task SelectDependencyFolderAsync(string folder)
    {
        await hideObfuscationInfoAlert();

        _dependenciesDirectoryName = folder;
    }
    public async Task SelectOutputDirectory(string folder)
    {
        await hideObfuscationInfoAlert();

        _outputDirectoryName = folder;
    }

    private async Task hideObfuscationInfoAlert()
    {
        await AlertsContainer.HideAlertAsync("obfuscation-info");
        StateHasChanged();
    }

    private async void onEnqueuedHandleAsync()
    {
        if (HandlerLogEventSink.Queue.TryDequeue(out var line))
        {
            await Highlight.WriteLineAsync(line);
        }
    }
    public async Task OnObfuscationFileChangeAsync(InputFileChangeEventArgs e)
    {
        await hideObfuscationInfoAlert();
        _obfuscationFile = e.File;
    }
}