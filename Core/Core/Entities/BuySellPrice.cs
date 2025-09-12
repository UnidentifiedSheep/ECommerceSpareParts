namespace Core.Entities;

public class BuySellPrice
{
    public decimal BuyPrice { get; set; }

    public decimal SellPrice { get; set; }

    public decimal? Markup { get; set; }

    public int CurrencyId { get; set; }

    public bool IsOutLiner { get; set; }

    public int SaleContentId { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual SaleContent SaleContent { get; set; } = null!;
}