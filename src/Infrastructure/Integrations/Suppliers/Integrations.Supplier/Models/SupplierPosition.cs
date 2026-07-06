namespace Integrations.Supplier.Models;

public record SupplierPosition
{
    public required string Id { get; init; }
    public PurchaseInfo? PurchaseInfo { get; init; }
    public DeliveryInfo? DeliveryInfo { get; init; }
}