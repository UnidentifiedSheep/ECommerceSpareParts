namespace Main.Application.Dtos.RepositoryOptionsData;

public record GetPurchaseOptionsData
{
    public required DateTime RangeStart { get; init; }
    public required DateTime RangeEnd { get; init; }
    public Guid? SupplierId { get; init; }
    public int? CurrencyId { get; init; }
    public string? SearchTerm { get; init; } 
}