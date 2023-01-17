namespace BitMono.Shared.Models;

public class ProtectionSetting
{
    [JsonRequired] public string Name { get; set; }
    public bool Enabled { get; set; }
    [JsonIgnore] public bool Disabled => Enabled == false;

    public void Enable()
    {
        Enabled = true;
    }
    public void Disable()
    {
        Enabled = false;
    }
}