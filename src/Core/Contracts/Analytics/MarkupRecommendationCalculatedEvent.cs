using System.Text.Json.Serialization;
using Contracts.Models.Markup;

namespace Contracts.Analytics;

public record MarkupRecommendationCalculatedEvent
{
    [JsonPropertyName("calculationJobId")]
    public Guid CalculationJobId { get; init; }

    [JsonPropertyName("currencyId")]
    public int CurrencyId { get; init; }

    [JsonPropertyName("ranges")]
    public IReadOnlyList<MarkupRecommendationRange> Ranges { get; init; } = [];
}