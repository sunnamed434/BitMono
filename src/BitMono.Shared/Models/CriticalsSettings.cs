namespace BitMono.Shared.Models;

public class CriticalsSettings
{
    public bool UseCriticalAttributes { get; set; }
    public bool UseCriticalModelAttributes { get; set; }
    public bool UseCriticalInterfaces { get; set; }
    public bool UseCriticalBaseTypes { get; set; }
    public bool UseCriticalMethodsStartsWith { get; set; }
    public bool UseCriticalMethods { get; set; }
#pragma warning disable CS8618
    public List<CriticalAttribute> CriticalAttributes { get; set; }
    public List<CriticalAttribute> CriticalModelAttributes { get; set; }
    public List<string> CriticalInterfaces { get; set; }
    public List<string> CriticalBaseTypes { get; set; }
    public List<string> CriticalMethodsStartsWith { get; set; }
    public List<string> CriticalMethods { get; set; }
#pragma warning restore CS8618
}