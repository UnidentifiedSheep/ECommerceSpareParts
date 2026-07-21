using System.Text.Json.Serialization;

namespace Pricing.Application.Lrts.InvalidateStalePriceOptions;

public record InvalidateStalePriceOptionsState
{
    [JsonPropertyName("processedRows")]
    public long ProcessedRows { get; init; } = 0;
}