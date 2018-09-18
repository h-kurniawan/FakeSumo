using Newtonsoft.Json;

namespace FakeSumo.Models
{
    public class SumoJobStatusResponse
    {
        public static readonly string[] States = new[] {
            "GATHERING RESULTS", "DONE GATHERING RESULTS", "NOT STARTED", "FORCE PAUSED", "CANCELLED"
        };

        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("messageCount")]
        public int MessageCount { get; set; }
    }
}
