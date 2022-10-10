using BitMono.API.Protecting;
using BitMono.Core.Configuration.Dependencies;
using BitMono.Core.Protecting;
using BitMono.GUI.API;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ILogger = Serilog.ILogger;

namespace BitMono.GUI.Pages
{
    public partial class Protect
    {
        private bool _obfuscating;
        private string _dependenciesFolder;
        private IBrowserFile _obfuscationFile;

        [Inject] public IFolderPicker FolderPicker { get; set; }
        [Inject] public ILogger Logger { get; set; }
        [Inject] public IConfiguration Configuration { get; set; }
        [Inject] public ICollection<IProtection> Protections { get; set; }
        [Inject] public IStoringProtections StoringProtections { get; set; }
        public string Message { get; set; }


        public async Task ObfuscateAsync()
        {
            if (_obfuscating == false)
            {
                try
                {
                    _obfuscating = true;
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
                    var protectionsWithConditions = protections.Where(p => p is ICallingCondition);
                    protections.Except(protectionsWithConditions);

                    var bitMonoAssemblyResolver = new BitMonoAssemblyResolver(Directory.GetFiles(_dependenciesFolder), protectionContext, Logger);
                    await bitMonoAssemblyResolver.ResolveAsync();
                    if (Configuration.GetValue<bool>("FailOnNoRequiredDependency"))
                    {
                        if (bitMonoAssemblyResolver.ResolvingFailed)
                        {
                            Logger.Warning("Drop dependencies in {0}, or set in config FailOnNoRequiredDependency to false", bitMonoContext.BaseDirectory);
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
                    bitMonoContext.ProtectedModuleFile = outputFile;

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
                            if (protection is ICallingCondition callingCondition)
                            {
                                if (callingCondition.Condition == CallingConditions.End)
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
                    _obfuscating = false;
                }
            }
        }
        public async Task ObfuscateFileAsync()
        {
            if (_obfuscationFile == null)
            {
                Message = "File not specified!";
                return;
            }

            await ObfuscateAsync();

            Message = "Obfuscated!";
        }
        public async Task PickDependenciesFolderAsync()
        {
            _dependenciesFolder = await FolderPicker.PickAsync();
        }

        public Task OnObfuscationFileChangeAsync(InputFileChangeEventArgs e)
        {
            _obfuscationFile = e.File;
            return Task.CompletedTask;
        }
    }
}