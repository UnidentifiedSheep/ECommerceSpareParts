using System.Text.Json.Serialization;
using Abstractions.Interfaces.Events;

namespace Contracts.ProductEnrichment;

public record CatalogueCandidateDeletedEvent : IKeyedEvent
{
	[JsonPropertyName("id")]
	public required Guid Id { get; init; }

	public string GetKey() => $"catalogue:candidate:{Id}:deleted";
}
