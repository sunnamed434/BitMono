namespace BitMono.Shared.Models;

[NullGuard(ValidationFlags.NonPublic)]
public class CriticalsSettings
{
    public bool UseCriticalAttributes { get; set; }
    public bool UseCriticalModelAttributes { get; set; }
    public bool UseCriticalInterfaces { get; set; }
    public bool UseCriticalBaseTypes { get; set; }
    public bool UseCriticalMethodsStartsWith { get; set; }
    public bool UseCriticalMethods { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public List<CriticalAttribute> CriticalAttributes { get; set; }
    public List<CriticalAttribute> CriticalModelAttributes { get; set; }
    public List<string> CriticalInterfaces { get; set; }
    public List<string> CriticalBaseTypes { get; set; }
    public List<string> CriticalMethodsStartsWith { get; set; }
    public List<string> CriticalMethods { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
