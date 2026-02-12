namespace Contracts.Sale;

public record SaleEditedEvent
{
    public Models.Sale.Sale Sale { get; init; } = null!;
    public IEnumerable<int> DeletedSaleContents { get; init; } = null!;
}