using System.Text.Json.Serialization;

namespace Main.Application.Lrts;

public sealed record BuildCatalogueCandidatesState
{
    [JsonPropertyName("lastProcessedId")]
    public int LastProcessedId { get; init; }

    [JsonPropertyName("processedRows")]
    public long ProcessedRows { get; init; }

    [JsonPropertyName("assignedRows")]
    public long AssignedRows { get; init; }

    [JsonPropertyName("skippedRows")]
    public long SkippedRows { get; init; }
}
