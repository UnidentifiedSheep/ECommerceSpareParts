using BulkValidation.Core.Attributes;
using Domain;

namespace Main.Entities.Storage;

public class StorageContent : AuditableEntity<StorageContent, int>
{
    [Validate]
    public int Id { get; set; }

    public string StorageName { get; set; } = null!;

    public int ProductId { get; set; }

    public int Count { get; set; }

    public decimal BuyPrice { get; set; }

    public int CurrencyId { get; set; }

    public decimal BuyPriceInUsd { get; set; }

    public DateTime PurchaseDatetime { get; set; }
    
    public Currency.Currency Currency { get; set; }
    
    public override int GetId() => Id;
}