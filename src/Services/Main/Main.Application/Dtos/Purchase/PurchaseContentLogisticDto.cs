using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Purchase;

public record PurchaseContentLogisticDto
{
    [JsonPropertyName("weightKg")]
    public required decimal WeightKg { get; init; }

    [JsonPropertyName("areaM3")]
    public required decimal AreaM3 { get; init; }

    [JsonPropertyName("price")]
    public required decimal Price { get; init; }
}