namespace BitMono.Core.Protecting;

public class RuntimeImplementations
{
    public RuntimeImplementations(IRenamer renamer, MscorlibInjector mscorlibInjector, CustomInjector customInjector)
    {
        Renamer = renamer;
        MscorlibInjector = mscorlibInjector;
        CustomInjector = customInjector;
        Random = new Random();
    }

    public IRenamer Renamer { get; }
    public MscorlibInjector MscorlibInjector { get; }
    public CustomInjector CustomInjector { get; }
    public Random Random { get; }
}