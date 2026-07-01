using System.Text.Json.Serialization;
using Main.Application.Dtos.Product;

namespace Main.Application.Dtos.Purchase;

public record PurchaseContentDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("count")]
    public required int Count { get; init; }

    [JsonPropertyName("price")]
    public required decimal Price { get; init; }

    [JsonPropertyName("totalSum")]
    public required decimal TotalSum { get; init; }

    [JsonPropertyName("comment")]
    public string? Comment { get; init; }

    [JsonPropertyName("product")]
    public required ProductDto Product { get; init; }

    [JsonPropertyName("logistics")]
    public required PurchaseContentLogisticDto? ContentLogistics { get; init; }
}