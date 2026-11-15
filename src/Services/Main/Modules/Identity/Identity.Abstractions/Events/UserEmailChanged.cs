using Domain.Interfaces.Events;

namespace Main.Modules.Identity.Abstractions;

public sealed record UserEmailChanged(Guid UserId) : IBatchableDomainEvent;
