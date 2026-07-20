using System.Text.Json.Serialization;

namespace Pricing.Application.Dtos.PriceApplier;

public record PriceApplierDto
{
    [JsonPropertyName("systemName")]
    public required string SystemName { get; init; }
    
    [JsonPropertyName("dslLogic")]
    public required string DslLogic { get; init; }
    
    [JsonPropertyName("states")]
    public required IReadOnlyList<PriceApplierStateDto> States { get; init; }
}