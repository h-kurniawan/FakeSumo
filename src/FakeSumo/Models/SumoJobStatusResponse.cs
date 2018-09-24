using Newtonsoft.Json;

namespace FakeSumo.Models
{
    public class SumoJobStatusResponse
    {
        public static readonly string[] States = new[] {
            "DONE GATHERING RESULTS", "GATHERING RESULTS", "NOT STARTED", "FORCE PAUSED", "CANCELLED"
        };

        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("messageCount")]
        public int MessageCount { get; set; }
    }
}
