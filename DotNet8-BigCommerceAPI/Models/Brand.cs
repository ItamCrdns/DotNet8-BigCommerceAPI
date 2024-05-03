using System.Text.Json.Serialization;

namespace BeachCommerce.Models
{
    public class Brand
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("page_title")]
        public string PageTitle { get; set; }

        [JsonPropertyName("meta_keywords")]
        public string[] MetaKeywords { get; set; }

        [JsonPropertyName("meta_description")]
        public string MetaDescription { get; set; }

        [JsonPropertyName("search_keywords")]
        public string SearchKeywords { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("custom_url")]
        public CustomUrl2 CustomUrl { get; set; }
    }

    public class CustomUrl2
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("is_default")]
        public bool IsDefault { get; set; }
    }
}
