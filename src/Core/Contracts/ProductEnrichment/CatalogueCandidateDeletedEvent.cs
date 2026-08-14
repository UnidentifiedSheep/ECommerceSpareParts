using System.Text.Json.Serialization;
using Abstractions.Interfaces.Events;

namespace Contracts.ProductEnrichment;

public record CatalogueCandidateDeletedEvent : IKeyedEvent
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    public string GetKey() => $"catalogue:candidate:{Id}:deleted";
}