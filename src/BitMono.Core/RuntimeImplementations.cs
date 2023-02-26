namespace BitMono.Core;

public class RuntimeImplementations
{
    public RuntimeImplementations(MscorlibInjector mscorlibInjector, CustomInjector customInjector)
    {
        MscorlibInjector = mscorlibInjector;
        CustomInjector = customInjector;
        Random = new Random();
    }

    public MscorlibInjector MscorlibInjector { get; }
    public CustomInjector CustomInjector { get; }
    public Random Random { get; }
}