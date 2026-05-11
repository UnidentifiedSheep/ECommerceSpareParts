using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Logistics;

public record LogisticsItemDto
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }

    [JsonPropertyName("quantity")]
    public required int Quantity { get; init; }
}