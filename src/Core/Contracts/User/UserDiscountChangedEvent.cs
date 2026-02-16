namespace Contracts.User;

public record UserDiscountChangedEvent
{
    public Guid UserId { get; init; }
    public decimal Discount { get; init; }
    public DateTime ChangedAt { get; init; }
}