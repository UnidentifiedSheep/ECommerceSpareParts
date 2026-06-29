using System.Text.Json.Serialization;

namespace Contracts.Sale.Model;

public record SaleContentEventModel
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("count")]
    public required int Count { get; init; }

    [JsonPropertyName("price")]
    public required decimal Price { get; init; }
    
    [JsonPropertyName("priceInBaseCurrency")]
    public required decimal PriceInBaseCurrency { get; init; }

    [JsonPropertyName("comment")]
    public string? Comment { get; init; }

    [JsonPropertyName("discount")]
    public required decimal Discount { get; init; }

    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }

    [JsonPropertyName("details")]
    public required IReadOnlyList<SaleContentDetailEventModel> Details { get; init; }
}
