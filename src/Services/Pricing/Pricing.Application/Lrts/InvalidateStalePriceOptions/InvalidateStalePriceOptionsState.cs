using System.Text.Json.Serialization;
using Application.Common.LRT;

namespace Pricing.Application.Lrts.InvalidateStalePriceOptions;

public record InvalidateStalePriceOptionsState : NoneInputState
{
    [JsonPropertyName("processedRows")]
    public long ProcessedRows { get; init; } = 0;
}
