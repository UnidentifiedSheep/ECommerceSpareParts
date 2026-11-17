using System.Text.Json.Serialization;

namespace Application.Common.LRT;

public sealed record SynchronizationState<TId> : NoneInputState where TId : struct
{
	[JsonPropertyName("lastProcessedId")]
	public TId? LastProcessedId { get; init; }

	[JsonPropertyName("processedCount")]
	public long ProcessedCount { get; init; }
}
