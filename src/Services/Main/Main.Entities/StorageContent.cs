using BulkValidation.Core.Attributes;

namespace Main.Entities;

public class StorageContent
{
    [Validate]
    public int Id { get; set; }

    public string StorageName { get; set; } = null!;

    public int ArticleId { get; set; }

    public int Count { get; set; }

    public decimal BuyPrice { get; set; }

    public int CurrencyId { get; set; }

    public decimal BuyPriceInUsd { get; set; }

    public DateTime CreatedDatetime { get; set; }

    public DateTime PurchaseDatetime { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual PurchaseContent? PurchaseContent { get; set; }

    public virtual ICollection<SaleContentDetail> SaleContentDetails { get; set; } = new List<SaleContentDetail>();

    public virtual Storage StorageNameNavigation { get; set; } = null!;
}