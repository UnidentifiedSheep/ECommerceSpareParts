namespace Analytics.Entities;

public partial class SaleContentDetail
{
    public int Id { get; set; }

    public int CurrencyId { get; set; }

    public decimal? BuyPrice { get; set; }

    public int Count { get; set; }

    public DateTime PurchaseDate { get; set; }

    public virtual Currency Currency { get; set; } = null!;
}
