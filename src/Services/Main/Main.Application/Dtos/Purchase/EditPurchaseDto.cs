using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Purchase;

public record EditPurchaseDto
{
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    [JsonPropertyName("productId")]
    public int ProductId { get; init; }

    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("price")]
    public decimal Price { get; init; }

    [JsonPropertyName("calculateLogistics")]
    public bool CalculateLogistics { get; init; }

    [JsonPropertyName("comment")]
    public string? Comment { get; init; }
}