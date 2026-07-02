using System.Text.Json.Serialization;

namespace Internal.Integration.Core.Models.Main.Product;

public record InternalFullProduct : InternalProduct
{
    [JsonPropertyName("weight")]
    public InternalProductWeight? ProductWeight { get; init; }

    [JsonPropertyName("size")]
    public InternalProductSize? ProductSize { get; init; }
}