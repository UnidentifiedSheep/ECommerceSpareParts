using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Product;

public record InternalFullProduct
{
    [JsonPropertyName("product")]
    public required InternalProduct Product { get; init; }

    [JsonPropertyName("productWeight")]
    public InternalProductWeight? ProductWeight { get; init; }

    [JsonPropertyName("productSize")]
    public InternalProductSize? ProductSize { get; init; }
}