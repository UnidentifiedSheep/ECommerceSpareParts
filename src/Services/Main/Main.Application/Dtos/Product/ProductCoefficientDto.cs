using System.Text.Json.Serialization;
using Main.Application.Dtos.Coefficient;

namespace Main.Application.Dtos.Product;

public record ProductCoefficientDto
{
    [JsonPropertyName("productId")]
    public required int ProductId { get; init; }

    [JsonPropertyName("validTill")]
    public required DateTime ValidTill { get; init; }

    [JsonPropertyName("createdAt")]
    public required DateTime CreatedAt { get; init; }

    [JsonPropertyName("updatedAt")]
    public required DateTime UpdatedAt { get; init; }

    [JsonPropertyName("coefficient")]
    public required CoefficientDto Coefficient { get; init; }
}