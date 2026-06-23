using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Purchase;

public record InternalFullPurchase
{
    [JsonPropertyName("purchase")]
    public required InternalPurchase Purchase { get; init; }

    [JsonPropertyName("contents")]
    public required IReadOnlyList<InternalPurchaseContent> Contents { get; init; }
}
