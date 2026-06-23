using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Purchase;

public record InternalPurchaseContentLogistic
{
    [JsonPropertyName("weightKg")]
    public required decimal WeightKg { get; init; }

    [JsonPropertyName("areaM3")]
    public required decimal AreaM3 { get; init; }

    [JsonPropertyName("price")]
    public required decimal Price { get; init; }
}
