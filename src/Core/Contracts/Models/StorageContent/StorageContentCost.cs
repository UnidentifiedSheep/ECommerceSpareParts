namespace Contracts.Models.StorageContent;

public class StorageContentCost
{
    public int ArticleId { get; set; }
    public int StorageContentId { get; set; }
    public string? PurchaseId { get; set; }
    public int? PurchaseContentId { get; set; }
    public int CurrencyId { get; set; }
    public int CurrentCount { get; set; }
    public decimal Price { get; set; }
    
    /// <summary>
    /// Delivery price PER ITEM.
    /// </summary>
    public decimal DeliveryPrice { get; set; }
    public int PurchaseContentCount { get; set; }
    public int DeliveryCurrencyId { get; set; }
    
    public DateTime PurchaseDatetime { get; set; }
}