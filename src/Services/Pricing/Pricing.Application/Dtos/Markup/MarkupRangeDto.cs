using System.Text.Json.Serialization;

namespace Pricing.Application.Dtos.Markup;

public record MarkupRangeDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("rangeStart")]
    public required decimal RangeStart { get; init; }

    [JsonPropertyName("rangeEnd")]
    public required decimal RangeEnd { get; init; }

    [JsonPropertyName("markup")]
    public required decimal Markup { get; init; }
}