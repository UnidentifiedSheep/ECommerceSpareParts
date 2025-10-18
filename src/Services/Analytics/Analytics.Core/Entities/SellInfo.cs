namespace Analytics.Core.Entities;

public partial class SellInfo
{
    public int ArticleId { get; set; }

    public string StorageName { get; set; } = null!;

    public int BuyCurrencyId { get; set; }

    public int SellCurrencyId { get; set; }

    public decimal BuyPrices { get; set; }

    public decimal SellPrice { get; set; }

    public int SellContentId { get; set; }

    public decimal? Markup { get; set; }

    public virtual Currency BuyCurrency { get; set; } = null!;

    public virtual Currency SellCurrency { get; set; } = null!;
}
