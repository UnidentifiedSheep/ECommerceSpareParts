using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.User;

public record UserUpdatedDomainEvent(Guid UserId) : IBatchableDomainEvent, IKeyedDomainEvent
{
	public string GetKey() => $"user:{UserId}:updated";
}
