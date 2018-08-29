using System.Collections.Generic;
using Newtonsoft.Json;

namespace FakeSumo.Models
{
    public class SumoMessageResponse
    {
        [JsonProperty("messages")]
        public List<SumoMessage> Messages { get; set; }
    }
}