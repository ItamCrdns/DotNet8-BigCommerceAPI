using System.Text.Json.Serialization;

namespace BeachCommerce.Models
{
    public class Link
    {
        [JsonPropertyName("next")]
        public string Next { get; set; }
        [JsonPropertyName("current")]
        public string Current { get; set; }
    }
}
