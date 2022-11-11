using BitMono.API.Configuration;
using BitMono.API.Protecting;
using BitMono.GUI.API;
using BitMono.GUI.Modules;
using BitMono.GUI.Shared.Alerting;
using BitMono.GUI.Shared.Inputs;
using BitMono.Obfuscation;
using dnlib.DotNet;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ILogger = Serilog.ILogger;

namespace BitMono.GUI.Pages.Obfuscation
{
    public partial class Protect
    {
        private string _dependenciesDirectory;
        private string _outputDirectory;
        private IBrowserFile _obfuscationFile;

        [Inject] public ILogger Logger { get; set; }
        [Inject] public IBitMonoAppSettingsConfiguration Configuration { get; set; }
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

        const string ExternalComponentsFile = nameof(BitMono) + "." + nameof(ExternalComponents) + ".dll";
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
                    var externalComponentsModuleDefMD = ModuleDefMD.Load(Path.Combine(baseDirectory, ExternalComponentsFile));

                    var obfuscationConfiguration = ServiceProvider.GetRequiredService<IBitMonoObfuscationConfiguration>();
                    var appSettingsConfiguration = ServiceProvider.GetRequiredService<IBitMonoAppSettingsConfiguration>();

                    var bitMonoContext = await new BitMonoContextCreator(obfuscationConfiguration).CreateAsync(_outputDirectory, _dependenciesDirectory);
                    bitMonoContext.ModuleFileName = _obfuscationFile.Name;
                    await new BitMonoObfuscator(ServiceProvider, new GUIModuleDefMDWriter(), new ModuleDefMDCreator(moduleBytes), Logger)
                        .ObfuscateAsync(bitMonoContext, externalComponentsModuleDefMD);

                    await new TipsNotifier(appSettingsConfiguration, Logger).NotifyAsync();
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
            if (_outputDirectory == null)
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

            _dependenciesDirectory = folder;
        }
        public async Task SelectOutputDirectory(string folder)
        {
            await hideObfuscationInfoAlert();

            _outputDirectory = folder;
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
}