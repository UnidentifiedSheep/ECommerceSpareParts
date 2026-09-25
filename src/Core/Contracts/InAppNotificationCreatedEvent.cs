using Abstractions.Interfaces.Events;

namespace Contracts;

public class InAppNotificationCreatedEvent : IKeyedEvent
{
	public int Id { get; init; }
	public Guid UserId { get; init; }
	public string GetKey() => $"notification:{Id}:created";
}
