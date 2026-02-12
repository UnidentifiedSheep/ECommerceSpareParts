namespace Contracts.Sale;

public record SaleCreatedEvent
{
    public Models.Sale.Sale Sale { get; init; } = null!;
}