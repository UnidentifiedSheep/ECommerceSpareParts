namespace Contracts.User;

public record UserDiscountUpdatedEvent
{
    public required Guid UserId { get; init; }
    public required decimal Discount { get; init; }
    public required DateTime ChangedAt { get; init; }
}