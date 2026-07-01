namespace Integrations.Supplier.Models;

public record PriceInfo
{
    public required decimal Price { get; init; }
    public required string CurrencyCode { get; init; }
}