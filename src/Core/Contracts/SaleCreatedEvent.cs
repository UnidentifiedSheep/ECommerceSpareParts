using System.Text.Json.Serialization;
using Contracts.Interfaces;

namespace Contracts;

public record SaleCreatedEvent : IKeyedEvent
{
	[JsonPropertyName("id")]
	public required Guid Id { get; init; }

	public string GetKey() => $"sale:{Id}:created";
}
