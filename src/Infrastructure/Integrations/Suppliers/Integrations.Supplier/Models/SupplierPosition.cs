namespace Integrations.Supplier.Models;

public record SupplierPosition
{
    public PurchaseInfo? PurchaseInfo { get; init; }
    public DeliveryInfo? DeliveryInfo { get; init; }
}