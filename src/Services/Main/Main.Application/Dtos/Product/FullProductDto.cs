using System.Text.Json.Serialization;

namespace Main.Application.Dtos.Product;

public record FullProductDto : ProductDto
{
    [JsonPropertyName("wight")]
    public required ProductWeightDto? ProductWeight { get; init; }

    [JsonPropertyName("size")]
    public required ProductSizeDto? ProductSize { get; init; }
}