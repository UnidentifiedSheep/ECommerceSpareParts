using Abstractions.Interfaces;

namespace Contracts.User;

public record UserPasswordChangedEvent : IKeyedEvent
{
    public required Guid UserId { get; init; }
    public string GetKey() => $"user-password-changed:{UserId}";
}