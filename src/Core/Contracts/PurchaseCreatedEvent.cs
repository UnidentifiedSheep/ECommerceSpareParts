using System.Text.Json.Serialization;
using Contracts.Interfaces;

namespace Contracts;

public record PurchaseCreatedEvent : IKeyedEvent
{
	[JsonPropertyName("id")]
	public required Guid Id { get; init; }
	public string GetKey() => $"purchase:{Id}:created";
}
