using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main;

public record InternalPurchaseContent
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
    public required InternalProduct Product { get; init; }

    [JsonPropertyName("logistics")]
    public required InternalPurchaseContentLogistic? ContentLogistics { get; init; }
}
