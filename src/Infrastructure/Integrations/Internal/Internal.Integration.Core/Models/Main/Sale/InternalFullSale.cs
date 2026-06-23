using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Sale;

public record InternalFullSale
{
    [JsonPropertyName("sale")]
    public required InternalSale Sale { get; init; }

    [JsonPropertyName("contents")]
    public required IReadOnlyList<InternalSaleContent> Contents { get; init; }
}
