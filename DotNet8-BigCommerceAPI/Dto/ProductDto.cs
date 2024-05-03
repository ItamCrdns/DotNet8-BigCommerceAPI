using System.Text.Json.Serialization;

namespace BeachCommerce.Dto
{
    public class ProductDto
    {
        // Required
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } // *
        [JsonPropertyName("type")]
        public string Type { get; set; } // *
        [JsonPropertyName("weight")] // *
        public decimal Weight { get; set; }
        [JsonPropertyName("price")] // *
        public decimal Price { get; set; }
        // Optional, but recommended
        [JsonPropertyName("brand_id")]
        public int BrandId { get; set; }
        //[JsonPropertyName("brand_name")] // *
        //public string BrandName { get; set; }
        [JsonPropertyName("sku")]
        public string Sku { get; set; } // *
        //[JsonPropertyName("description")]
        //public string Description { get; set; }
        [JsonPropertyName("inventory_level")]
        public int InventoryLevel { get; set; } // *
    }
}
