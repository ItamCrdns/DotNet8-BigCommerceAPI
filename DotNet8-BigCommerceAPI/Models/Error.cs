using System.Text.Json.Serialization;

namespace BeachCommerce.Models
{
    public class Error
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("instance")]
        public string Instance { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("errors")]
        public object Errors { get; set; }
    }
}
