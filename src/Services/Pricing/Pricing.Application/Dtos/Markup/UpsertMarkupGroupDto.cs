using System.Text.Json.Serialization;

namespace Pricing.Application.Dtos.Markup;

public record UpsertMarkupGroupDto
{
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("currencyId")]
    public required int CurrencyId { get; init; }

    [JsonPropertyName("ranges")]
    public required IReadOnlyList<UpsertMarkupRangeDto> Ranges { get; init; }
}

public record UpsertMarkupRangeDto
{
    [JsonPropertyName("rangeStart")]
    public required decimal RangeStart { get; init; }

    [JsonPropertyName("rangeEnd")]
    public required decimal RangeEnd { get; init; }

    [JsonPropertyName("markup")]
    public required decimal Markup { get; init; }
}