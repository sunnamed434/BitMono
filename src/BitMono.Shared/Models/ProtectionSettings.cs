namespace BitMono.Shared.Models;

public class ProtectionSettings
{
    [AllowNull]
    public List<ProtectionSetting> Protections { get; set; }
}