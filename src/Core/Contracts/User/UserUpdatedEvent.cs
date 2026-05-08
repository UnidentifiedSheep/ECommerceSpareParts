using Abstractions.Interfaces;

namespace Contracts.User;

public record UserUpdatedEvent : IKeyedEvent
{
    public required Guid UserId { get; init; }

    public string GetKey()
    {
        return $"user-updated:{UserId}";
    }
}