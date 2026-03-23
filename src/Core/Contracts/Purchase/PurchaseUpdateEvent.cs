namespace Contracts.Purchase;

public record PurchaseUpdateEvent
{
    public required Models.Purchase.Purchase Purchase { get; init; }
}