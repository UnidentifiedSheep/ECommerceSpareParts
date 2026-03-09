namespace Contracts.Purchase;

public class PurchaseCreatedEvent
{
    public required Models.Purchase.Purchase Purchase { get; init; }
}