using System.Text.Json.Serialization;

namespace BeachCommerce.Models
{
    public class Meta
    {
        [JsonPropertyName("pagination")]
        public Pagination Pagination { get; set; }
    }
}
