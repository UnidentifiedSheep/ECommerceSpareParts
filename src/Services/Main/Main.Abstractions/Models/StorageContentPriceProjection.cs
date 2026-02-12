namespace Main.Abstractions.Models;

public class StorageContentPriceProjection
{
    public int ArticleId { get; set; }
    public int StorageContentId { get; set; }
    public decimal Price { get; set; }
    public int CurrentCount { get; set; }
    public int CurrencyId { get; set; }
    public decimal? LogisticsPrice { get; set; }
    public int? PurchaseContentId { get; set; }
    public string? PurchaseId { get; set; }
    public int? LogisticsCurrencyId { get; set; }
    public int? PurchaseContentCount { get; set; }
    public DateTime PurchaseDatetime { get; set; }
}