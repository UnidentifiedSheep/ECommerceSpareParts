namespace Main.Entities;

public partial class SaleContentDetail
{
    public int Id { get; set; }

    public int SaleContentId { get; set; }

    public int? StorageContentId { get; set; }

    public string Storage { get; set; } = null!;

    public int CurrencyId { get; set; }

    public decimal BuyPrice { get; set; }

    public int Count { get; set; }

    public DateTime PurchaseDatetime { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual SaleContent SaleContent { get; set; } = null!;

    public virtual StorageContent? StorageContent { get; set; }

    public virtual Storage StorageNavigation { get; set; } = null!;
}
