using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.User;

public record UserEmailUpdatedDomainEvent(Guid UserId) : IBatchableDomainEvent, IKeyedDomainEvent
{
	public string GetKey() => $"user:{UserId}:email:updated";
}
