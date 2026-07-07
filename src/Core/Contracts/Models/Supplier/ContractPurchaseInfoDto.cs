namespace Contracts.Models.Supplier;

public record ContractPurchaseInfoDto
{
    public int AvailableQuantity { get; init; }
    public int MinimumPurchaseQuantity { get; init; }
    public int QuantityCoefficient { get; init; }

    public int DaysToRefund { get; init; }
    public bool PartnerWarehouse { get; init; } //Is the product in the partner warehouse

    public required ContractPriceInfoDto PriceInfo { get; init; }
}