using System.Text.Json.Serialization;

namespace Contracts.Analytics;

public record MarkupAnalyzedEvent
{
    [JsonPropertyName("ranges")]
    public required IReadOnlyList<MarkupRangeItem> Ranges { get; init; }
}

public record MarkupRangeItem
{
    [JsonPropertyName("fromCost")]
    public required decimal FromCost { get; init; }

    [JsonPropertyName("toCost")]
    public required decimal ToCost { get; init; }

    [JsonPropertyName("meanMarkup")]
    public required decimal MeanMarkup { get; init; }

    [JsonPropertyName("stdDevMarkup")]
    public required decimal StdDevMarkup { get; init; }

    [JsonPropertyName("count")]
    public required int Count { get; init; }
}