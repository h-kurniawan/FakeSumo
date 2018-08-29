using Newtonsoft.Json;

namespace FakeSumo.Models
{
    public class SumoMessage
    {
        [JsonProperty("map")]
        public SumoMessageMap Map { get; set; }
    }
}