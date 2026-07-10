namespace Contracts.Models.Supplier;

public record ContractSupplierPositionDto
{
    public required string Id { get; init; }
    public ContractPurchaseInfoDto? PurchaseInfo { get; init; }
    public ContractDeliveryInfoDto? DeliveryInfo { get; init; }
}