using System.Text.Json.Serialization;

namespace BeachCommerce.Models
{
    public class Product
    {
        // Propiedades marcadas con JsonPropertyName son las necesarias para esta prueba
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("sku")]
        public string Sku { get; set; }
        public string Description { get; set; }
        [JsonPropertyName("weight")]
        public decimal Weight { get; set; }
        public decimal Width { get; set; }
        public decimal Depth { get; set; }
        public decimal Height { get; set; }
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal MapPrice { get; set; }
        public int TaxClassId { get; set; }
        public string ProductTaxCode { get; set; }
        public decimal CalculatedPrice { get; set; }
        public int[] Categories { get; set; }
        [JsonPropertyName("brand_id")]
        public int BrandId { get; set; }
        [JsonPropertyName("brand_name")]
        public string BrandName { get; set; }
        public int? OptionSetId { get; set; }
        public string OptionSetDisplay { get; set; }
        [JsonPropertyName("inventory_level")]
        public int InventoryLevel { get; set; }
        public int InventoryWarningLevel { get; set; }
        public string InventoryTracking { get; set; }
        public int ReviewsRatingSum { get; set; }
        public int ReviewsCount { get; set; }
        public int TotalSold { get; set; }
        public decimal FixedCostShippingPrice { get; set; }
        public bool IsFreeShipping { get; set; }
        public bool IsVisible { get; set; }
        public bool IsFeatured { get; set; }
        public int[] RelatedProducts { get; set; }
        public string Warranty { get; set; }
        public string BinPickingNumber { get; set; }
        public string LayoutFile { get; set; }
        public string Upc { get; set; }
        public string Mpn { get; set; }
        public string Gtin { get; set; }
        public DateTime? DateLastImported { get; set; }
        public string SearchKeywords { get; set; }
        public string Availability { get; set; }
        public string AvailabilityDescription { get; set; }
        public string GiftWrappingOptionsType { get; set; }
        public object[] GiftWrappingOptionsList { get; set; }
        public int SortOrder { get; set; }
        public string Condition { get; set; }
        public bool IsConditionShown { get; set; }
        public int OrderQuantityMinimum { get; set; }
        public int OrderQuantityMaximum { get; set; }
        public string PageTitle { get; set; }
        public string[] MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public int ViewCount { get; set; }
        public DateTime? PreorderReleaseDate { get; set; }
        public string PreorderMessage { get; set; }
        public bool IsPreorderOnly { get; set; }
        public bool IsPriceHidden { get; set; }
        public string PriceHiddenLabel { get; set; }
        public CustomUrl CustomUrl { get; set; }
        public int BaseVariantId { get; set; }
        public string OpenGraphType { get; set; }
        public string OpenGraphTitle { get; set; }
        public string OpenGraphDescription { get; set; }
        public bool OpenGraphUseMetaDescription { get; set; }
        public bool OpenGraphUseProductName { get; set; }
        public bool OpenGraphUseImage { get; set; }
    }
    public class CustomUrl
    {
        public string Url { get; set; }
        public bool IsCustomized { get; set; }
    }
}
