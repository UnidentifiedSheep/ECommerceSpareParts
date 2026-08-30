using System.Text.Json.Serialization;
using Application.Common.LRT;

namespace Main.Application.Lrts.BuildCatalogueCandidates;

public sealed record BuildCatalogueCandidatesState : NoneInputState
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
