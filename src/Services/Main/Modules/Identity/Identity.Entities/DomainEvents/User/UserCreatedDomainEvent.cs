using Domain.Interfaces.Events;

namespace Main.Entities.DomainEvents.User;

public record UserCreatedDomainEvent(Guid UserId, string Name, string Surname) : IBatchableDomainEvent;
