using Newtonsoft.Json;

namespace FakeSumo.Models
{
    public class SumoJobStatusResponse
    {
        public static readonly string[] States = new[] {
            "NOT STARTED", "GATHERING RESULTS", "FORCE PAUSED", "DONE GATHERING RESULTS", "CANCELLED"
        };

        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("messageCount")]
        public int MessageCount { get; set; }
    }
}
