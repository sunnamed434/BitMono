namespace BitMono.Shared.Models;

public class Criticals
{
    public bool UseCriticalAttributes { get; set; }
    public bool UseCriticalModelAttributes { get; set; }
    public bool UseCriticalInterfaces { get; set; }
    public bool UseCriticalBaseTypes { get; set; }
    public bool UseCriticalMethods { get; set; }
    [AllowNull]
    public List<CriticalAttribute> CriticalAttributes { get; set; }
    [AllowNull]
    public List<CriticalAttribute>? CriticalModelAttributes { get; set; }
    [AllowNull]
    public List<string>? CriticalInterfaces { get; set; }
    [AllowNull]
    public List<string>? CriticalBaseTypes { get; set; }
    [AllowNull]
    public List<string>? CriticalMethods { get; set; }
}