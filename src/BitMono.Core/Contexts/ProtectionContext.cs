namespace BitMono.Core.Contexts;

public class ProtectionContext
{
    public ProtectionContext(ModuleDefinition module, ModuleDefinition runtimeModule, BitMonoContext bitMonoContext,
        ProtectionParameters parameters, CancellationToken cancellationToken)
    {
        Module = module;
        RuntimeModule = runtimeModule;
        BitMonoContext = bitMonoContext;
        Parameters = parameters;
        CancellationToken = cancellationToken;
    }

    public ModuleDefinition Module { get; }
    public ModuleDefinition RuntimeModule { get; }
    public BitMonoContext BitMonoContext { get; }
    public ProtectionParameters Parameters { get; }
    public CancellationToken CancellationToken { get; }

    public ReferenceImporter ModuleImporter => Module.DefaultImporter;
    public ReferenceImporter RuntimeImporter => Module.DefaultImporter;
    public bool X86 => Module.MachineType == MachineType.I386;

    public void ThrowIfCancellationTokenRequested()
    {
        CancellationToken.ThrowIfCancellationRequested();
    }
    /// <summary>
    /// This is necessary to make native code work inside the assembly.
    /// See more here: https://docs.washi.dev/asmresolver/guides/dotnet/unmanaged-method-bodies.html
    /// However, sometimes it causes issues with the assembly like `System.BadImageFormatException`
    /// at the end when running the protected file, so that's why it's here but not at some startup point.
    /// </summary>
    public void ConfigureForNativeCode()
    {
        Module.IsILOnly = false;
        var x64 = Module.MachineType == MachineType.Amd64;
        if (x64)
        {
            Module.PEKind = OptionalHeaderMagic.PE32Plus;
            Module.MachineType = MachineType.Amd64;
            Module.IsBit32Required = false;
        }
        else
        {
            Module.PEKind = OptionalHeaderMagic.PE32;
            Module.MachineType = MachineType.I386;
            Module.IsBit32Required = true;
        }
    }
}