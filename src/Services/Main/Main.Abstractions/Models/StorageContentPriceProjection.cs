namespace Main.Abstractions.Models;

public record StorageContentPriceProjection
{
    public required int ProductId { get; init; }
    public required int StorageContentId { get; init; }
    public required decimal Price { get; init; }
    public required int CurrentCount { get; init; }
    public required int CurrencyId { get; init; }
    public required decimal? LogisticsPrice { get; init; }
    public required int? PurchaseContentId { get; init; }
    public required string? PurchaseId { get; init; }
    public required int? LogisticsCurrencyId { get; init; }
    public required int? PurchaseContentCount { get; init; }
    public required DateTime PurchaseDatetime { get; init; }
}