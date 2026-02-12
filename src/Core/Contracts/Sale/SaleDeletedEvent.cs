namespace Contracts.Sale;

public record SaleDeletedEvent
{
    public Models.Sale.Sale Sale { get; init; } = null!;
}