using System.Text.Json.Serialization;

namespace Pricing.Application.Dtos.Markups;

public record NewMarkupRangeDto
{
    [JsonPropertyName("rangeStart")]
    public decimal RangeStart { get; init; }

    [JsonPropertyName("rangeEnd")]
    public decimal RangeEnd { get; init; }

    [JsonPropertyName("markupRate")]
    public decimal MarkupRate { get; init; }
}