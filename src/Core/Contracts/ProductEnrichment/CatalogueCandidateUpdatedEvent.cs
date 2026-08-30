using System.Text.Json.Serialization;
using Abstractions.Interfaces.Events;
using Contracts.Models.CatalogueCandidate;

namespace Contracts.ProductEnrichment;

public record CatalogueCandidateUpdatedEvent : IKeyedEvent
{
	[JsonPropertyName("candidate")]
	public required CatalogueCandidateContractDto Candidate { get; init; }

	[JsonPropertyName("occuredAt")]
	public required DateTime OccuredAt { get; init; }

	public string GetKey() => $"catalogue:candidate:{Candidate.Id}:updated";
}
