namespace Main.Application.Dtos.RepositoryOptionsData;

public record GetSalesOptionsData
{
    public required DateTime RangeStart { get; init; } 
    public required DateTime RangeEnd { get; init; } 
    public string? SearchTerm { get; init; } 
    public Guid? BuyerId { get; init; } 
    public int? CurrencyId { get; init; } 
}