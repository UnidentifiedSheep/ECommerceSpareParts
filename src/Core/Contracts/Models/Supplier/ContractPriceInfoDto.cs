namespace Contracts.Models.Supplier;

public record ContractPriceInfoDto
{
    public required decimal Price { get; init; }
    public required string CurrencyCode { get; init; }
}