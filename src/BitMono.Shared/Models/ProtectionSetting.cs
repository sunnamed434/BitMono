namespace BitMono.Shared.Models;

public class ProtectionSetting
{
    // ponytail: dropped [JsonRequired] — STJ v6 (net6.0 in-box) has no such attribute, and a nameless
    // entry just fails to match a protection (no crash). Restore via a conditional alias if you want the throw.
    public string Name { get; set; }
    public bool Enabled { get; set; }
    [JsonIgnore] public bool Disabled => !Enabled;

    public void Enable()
    {
        Enabled = true;
    }
    public void Disable()
    {
        Enabled = false;
    }
}