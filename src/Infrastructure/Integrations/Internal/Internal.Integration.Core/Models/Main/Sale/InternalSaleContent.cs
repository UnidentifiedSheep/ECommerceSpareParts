using System.Text.Json.Serialization;
using Internal.Integration.Core.Models.Main.Product;

namespace Internal.Integration.Core.Models.Main.Sale;

public record InternalSaleContent
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

    [JsonPropertyName("discount")]
    public required decimal Discount { get; init; }

    [JsonPropertyName("product")]
    public required InternalProduct Product { get; init; }

    [JsonPropertyName("details")]
    public required IReadOnlyList<InternalSaleContentDetail> Details { get; init; }
}