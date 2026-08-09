using System.Text.Json.Serialization;
using Application.Common.LRT;

namespace Main.Application.Lrts;

public sealed record MapCatalogueCandidatesToProductsState : NoneInputState
{
    [JsonPropertyName("lastProcessedId")]
    public int LastProcessedId { get; init; }

    [JsonPropertyName("processedRows")]
    public long ProcessedRows { get; init; }

    [JsonPropertyName("mappedRows")]
    public long MappedRows { get; init; }

    [JsonPropertyName("skippedRows")]
    public long SkippedRows { get; init; }
}
