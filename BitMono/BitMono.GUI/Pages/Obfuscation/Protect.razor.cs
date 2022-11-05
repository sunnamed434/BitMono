using BitMono.API.Protecting;
using BitMono.API.Protecting.Context;
using BitMono.API.Protecting.Pipeline;
using BitMono.Core.Configuration.Dependencies;
using BitMono.Core.Models;
using BitMono.Core.Protecting.Resolvers;
using BitMono.GUI.API;
using BitMono.GUI.Shared.Alerting;
using BitMono.GUI.Shared.Inputs;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using ILogger = Serilog.ILogger;

namespace BitMono.GUI.Pages.Obfuscation
{
    public partial class Protect
    {
        private string _dependenciesFolder;
        private IBrowserFile _obfuscationFile;

        [Inject] public ILogger Logger { get; set; }
        [Inject] public IConfiguration Configuration { get; set; }
        [Inject] public ICollection<IProtection> Protections { get; set; }
        [Inject] public IStoringProtections StoringProtections { get; set; }
        public Highlight Highlight { get; set; }
        public bool ObfuscationInProcess { get; set; }


        protected override Task OnInitializedAsync()
        {
            HandlerLogEventSink.OnEnqueued += onEnqueuedHandleAsync;
            return Task.CompletedTask;
        }

        public async Task ObfuscateAsync()
        {
            if (ObfuscationInProcess == false)
            {
                try
                {
                    ObfuscationInProcess = true;
                    var domainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    const string EncryptionFile = "BitMono.Encryption.dll";
                    var encryptionModuleDefMD = ModuleDefMD.Load(Path.Combine(domainBaseDirectory, EncryptionFile));

                    var baseDirectory = Path.Combine(domainBaseDirectory, "Base");
                    var outputDirectory = Path.Combine(domainBaseDirectory, "Output");
                    Logger.Information(baseDirectory);
                    Logger.Information(outputDirectory);
                    Directory.CreateDirectory(baseDirectory);
                    Directory.CreateDirectory(outputDirectory);
                    var bitMonoContext = new BitMonoContext
                    {
                        BaseDirectory = domainBaseDirectory,
                        OutputDirectory = outputDirectory,
                        Watermark = Configuration.GetValue<bool>(nameof(BitMonoContext.Watermark)),
                    };

                    bitMonoContext.ModuleFile = _obfuscationFile.Name;

                    var memoryStream = new MemoryStream();
                    await _obfuscationFile.OpenReadStream().CopyToAsync(memoryStream);
                    var moduleBytes = memoryStream.ToArray();

                    var assemblyResolver = new AssemblyResolver();
                    var moduleContext = new ModuleContext(assemblyResolver);
                    assemblyResolver.DefaultModuleContext = moduleContext;

                    var moduleCreationOptions = new ModuleCreationOptions(assemblyResolver.DefaultModuleContext, CLRRuntimeReaderKind.Mono);
                    var moduleDefMD = ModuleDefMD.Load(moduleBytes, moduleCreationOptions);
                    var moduleWriterOptions = new ModuleWriterOptions(moduleDefMD);
                    moduleWriterOptions.MetadataLogger = DummyLogger.NoThrowInstance;
                    moduleWriterOptions.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack | MetadataFlags.PreserveAll;
                    moduleWriterOptions.Cor20HeaderOptions.Flags = ComImageFlags.ILOnly;

                    Assembly moduleAssembly = null;
                    try
                    {
                        moduleAssembly = Assembly.Load(moduleBytes);
                    }
                    catch (FileLoadException ex)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            Logger.Fatal("Seems Module {0}, is blocked, try to unblock it (disabling the security) by opening propetries of the file, " +
                                "should be checked 'Unblock' & then pressed button 'Apply'", bitMonoContext.ModuleFile);
                        }
                        else
                        {
                            Logger.Fatal(ex, "Failed to load module assembly!");
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal(ex, "Failed to load module assembly!");
                        return;
                    }

                    var protectionContext = new ProtectionContext
                    {
                        ModuleDefMD = moduleDefMD,
                        ModuleCreationOptions = moduleCreationOptions,
                        ModuleWriterOptions = moduleWriterOptions,
                        EncryptionModuleDefMD = encryptionModuleDefMD,
                        Assembly = Assembly.Load(moduleBytes),
                        BitMonoContext = bitMonoContext,
                    };

                    Logger.Information("Loaded Module {0}", moduleDefMD.Name);
                    Logger.Warning("Resolving dependecies in {0}", bitMonoContext.ModuleFile);

                    var protections = Protections;
                    protections = new DependencyResolver(protections, StoringProtections.Protections, Logger)
                        .Sort(out ICollection<string> skipped);
                    var protectionsWithConditions = protections.Where(p => p is IPipelineStage);
                    protections.Except(protectionsWithConditions);

                    var bitMonoAssemblyResolver = new BitMonoAssemblyResolver(Directory.GetFiles(_dependenciesFolder), protectionContext, Logger);
                    var resolvingSucceed = await bitMonoAssemblyResolver.ResolveAsync();
                    if (Configuration.GetValue<bool>(nameof(AppSettings.FailOnNoRequiredDependency)))
                    {
                        if (resolvingSucceed == false)
                        {
                            Logger.Warning("Drop dependencies in {0}, or set in config 'FailOnNoRequiredDependency' to false to ignore this", bitMonoContext.BaseDirectory);
                            return;
                        }
                    }
                    Logger.Information("Protecting: {0}", bitMonoContext.ModuleFile);

                    if (skipped.Any())
                    {
                        Logger.Warning("Skip protections: {0}", string.Join(", ", skipped.Select(p => p ?? "NULL")));
                    }

                    if (protections.Any())
                    {
                        Logger.Information("Execute protections: {0}", string.Join(", ", protections.Select(p => p.GetType().Name ?? "NULL")));
                    }

                    if (protectionsWithConditions.Any())
                    {
                        Logger.Information("Execute calling condition protections: {0}", string.Join(", ", protectionsWithConditions.Select(p => p.GetType().Name ?? "NULL")));
                    }

                    foreach (var protection in protections.Except(protectionsWithConditions))
                    {
                        try
                        {
                            Logger.Information("{0} -> Executing.. ", protection.GetType().FullName);
                            await protection.ExecuteAsync(protectionContext);
                            Logger.Information("{0} -> Executed! ", protection.GetType().FullName);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Error while executing protections!");
                        }
                    }

                    var stringBuilder = new StringBuilder()
                        .Append(Path.GetFileNameWithoutExtension(bitMonoContext.ModuleFile));

                    if (bitMonoContext.Watermark)
                    {
                        stringBuilder.
                            Append("_bitmono");
                    }

                    stringBuilder.Append(Path.GetExtension(bitMonoContext.ModuleFile));

                    var outputFile = Path.Combine(bitMonoContext.OutputDirectory, stringBuilder.ToString());
                    bitMonoContext.OutputModuleFile = outputFile;

                    try
                    {
                        using (moduleDefMD)
                        using (var fileStream = File.Create(outputFile))
                        {
                            moduleDefMD.Write(fileStream, moduleWriterOptions);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error while saving module!");
                    }

                    try
                    {
                        foreach (var protection in protectionsWithConditions)
                        {
                            if (protection is IPipelineStage callingCondition)
                            {
                                if (callingCondition.Stage == PipelineStages.ModuleWrite)
                                {
                                    Logger.Information("{0} -> Executing.. ", protection.GetType().FullName);
                                    await protection.ExecuteAsync(protectionContext);
                                    Logger.Information("{0} -> Executed! ", protection.GetType().FullName);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error while executing Calling condition protections!");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex, "Errore!");
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

            await ObfuscateAsync();
            await AlertsContainer.ShowAlertAsync("obfuscation-info", "Protected!", Alerts.Success);
        }
        public async Task SelectDependencyFolderAsync(string folder)
        {
            await hideObfuscationInfoAlert();

            _dependenciesFolder = folder;
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