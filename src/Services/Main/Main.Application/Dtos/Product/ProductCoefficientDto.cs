using System.Text.Json.Serialization;
using Main.Application.Dtos.Amw.Coefficients;

namespace Main.Application.Dtos.Amw.ArticleCoefficients;

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