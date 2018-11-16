using System.Runtime.Serialization;

namespace FakeSumo.Models
{
    [DataContract]
    public class SumoJobSearchResponse
    {
        [DataContract]
        public class HypermediaLink
        {
            [DataMember(Name = "rel")]
            public string Rel { get; set; }

            [DataMember(Name = "href")]
            public string SearchLocation { get; set; }
        }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "link")]
        public HypermediaLink Link { get; set; }
    }
}
