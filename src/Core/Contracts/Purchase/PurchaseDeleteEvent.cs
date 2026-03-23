namespace Contracts.Purchase;

public record PurchaseDeleteEvent
{
    public required Models.Purchase.Purchase Purchase { get; init; }
}