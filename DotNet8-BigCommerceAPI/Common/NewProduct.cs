using System.Text.Json.Serialization;

namespace BeachCommerce.Common
{
    public class NewProduct
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("sku")]
        public string Sku { get; set; }
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        [JsonPropertyName("weight")]
        public decimal Weight { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("inventory_level")]
        public int InventoryLevel { get; set; }
        [JsonPropertyName("brand_name")]
        public string BrandName { get; set; }
    }
}
