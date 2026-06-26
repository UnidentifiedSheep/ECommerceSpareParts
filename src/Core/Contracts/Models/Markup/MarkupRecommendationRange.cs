using System.Text.Json.Serialization;

namespace Contracts.Models.Markup;

public record MarkupRecommendationRange
{
    [JsonPropertyName("from")]
    public required decimal From { get; init; }
    
    [JsonPropertyName("to")]
    public required decimal To { get; init; }
    
    [JsonPropertyName("markup")]
    public required decimal Markup { get; init; }
}