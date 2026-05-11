using Domain;

namespace Analytics.Entities;

public class SaleContentDetail : Entity<SaleContentDetail, int>
{
    public int Id { get; set; }

    public int CurrencyId { get; set; }

    public decimal? BuyPrice { get; set; }

    public int Count { get; set; }

    public DateTime PurchaseDate { get; set; }

    public override int GetId()
    {
        return Id;
    }
}