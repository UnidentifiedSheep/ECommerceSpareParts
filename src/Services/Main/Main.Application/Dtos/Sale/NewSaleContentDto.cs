using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Sale;

public record NewSaleContentDto
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }

    [JsonPropertyName("count")]
    public required int Count { get; init; }

    [JsonPropertyName("price")]
    public required decimal Price { get; init; }

    [JsonPropertyName("priceWithDiscount")]
    public required decimal PriceWithDiscount { get; init; }

    [JsonPropertyName("comment")]
    public string? Comment { get; init; }
}