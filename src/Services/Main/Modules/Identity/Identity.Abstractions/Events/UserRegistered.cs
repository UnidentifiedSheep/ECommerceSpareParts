using Domain.Interfaces.Events;

namespace Main.Modules.Identity.Abstractions;

public sealed record UserRegistered(Guid UserId, string Name, string Surname) : IBatchableDomainEvent;
