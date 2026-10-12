using System.Text.Json.Serialization;
using Application.Common.LRT;

namespace Main.Application.Lrts.Base.Synchronization;

public sealed record SynchronizationState<TId> : NoneInputState where TId : struct
{
	[JsonPropertyName("lastProcessedId")]
	public TId? LastProcessedId { get; init; }
}
