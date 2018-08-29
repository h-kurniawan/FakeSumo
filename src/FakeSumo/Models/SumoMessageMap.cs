using Newtonsoft.Json;

namespace FakeSumo.Models
{
    public class SumoMessageMap
    {
        [JsonProperty("_raw")]
        public string RawMessage { get; set; }
    }
}