using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.User;

public record UserEmailsUpdatedDomainEvent(Guid UserId) : IBatchableDomainEvent, IKeyedDomainEvent
{
    public string GetKey() => $"user:{UserId}:emails:updated"; 
}