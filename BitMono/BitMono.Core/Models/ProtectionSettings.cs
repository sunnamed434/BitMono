using Newtonsoft.Json;

namespace BitMono.Core.Models
{
    public class ProtectionSettings
    {
        [JsonRequired]
        public string Name { get; set; }
        public bool Enabled { get; set; }
        [JsonIgnore]
        public bool Disabled => Enabled == false;
    }
}