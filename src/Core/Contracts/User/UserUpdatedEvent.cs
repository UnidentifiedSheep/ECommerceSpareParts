namespace Contracts.User;

public record UserUpdatedEvent
{
    public required Guid UserId { get; init; }
}