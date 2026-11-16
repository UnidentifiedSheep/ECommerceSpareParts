using Domain.Interfaces.Events;

namespace Main.Modules.Identity.Abstractions.Events;

public sealed record UserEmailChanged(Guid UserId) : IBatchableDomainEvent;
