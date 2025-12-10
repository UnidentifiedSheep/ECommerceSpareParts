namespace Main.Core.Entities;

public partial class ArticleSupplierBuyInfo
{
    public int Id { get; set; }

    public int ArticleId { get; set; }

    public Guid WhoProposed { get; set; }

    public int CurrencyId { get; set; }

    public decimal BuyPrice { get; set; }

    public int DeliveryIdDays { get; set; }

    public DateTime CreationDatetime { get; set; }

    public int CurrentSupplierStock { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual User WhoProposedNavigation { get; set; } = null!;
}
