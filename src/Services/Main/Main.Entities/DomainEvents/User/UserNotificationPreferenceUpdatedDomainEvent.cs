using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.User;

public record UserNotificationPreferenceUpdatedDomainEvent(Guid UserId) : IBatchableDomainEvent, IKeyedDomainEvent
{
	public string GetKey() => $"user:{UserId}:notification:preferences:updated";
}
