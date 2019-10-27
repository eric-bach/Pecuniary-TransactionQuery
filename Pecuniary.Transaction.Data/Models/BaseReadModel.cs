using Newtonsoft.Json;

namespace Pecuniary.Transaction.Data.Models
{
    /// <summary>
    /// Based on the ElasticSearch response model
    /// </summary>
    public class BaseReadModel
    {
        [JsonProperty("_index")]
        public string Index { get; set; }

        [JsonProperty("_type")]
        public string Type { get; set; }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("_version")]
        public int Version { get; set; }

        [JsonProperty("found")]
        public bool Found { get; set; }
    }
}
