using System.Text.Json.Serialization;

namespace BeachCommerce.Models
{
    public class MetaData<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
        [JsonPropertyName("meta")]
        public Meta Meta { get; set; }
        //public Error? Errors { get; set; } // Not always returned
    }
}
