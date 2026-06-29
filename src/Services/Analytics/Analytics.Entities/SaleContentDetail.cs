using Domain;
using Domain.Extensions;
using Exceptions;

namespace Analytics.Entities;

public class SaleContentDetail : Entity<SaleContentDetail, int>
{
    private SaleContentDetail()
    {
    }

    public int Id { get; private set; }

    public int SaleContentId { get; private set; }

    public int CurrencyId { get; private set; }

    public decimal? BuyPrice { get; private set; }

    public int Count { get; private set; }

    public DateTime PurchaseDate { get; private set; }

    public virtual SaleContent SaleContent { get; private set; } = null!;

    public static SaleContentDetail Create(
        int id,
        int saleContentId,
        int currencyId,
        decimal? buyPrice,
        int count,
        DateTime purchaseDate)
    {
        var detail = new SaleContentDetail
        {
            Id = id,
            SaleContentId = saleContentId,
            CurrencyId = currencyId
        };

        detail.Update(currencyId, buyPrice, count, purchaseDate);
        return detail;
    }

    public void Update(
        int currencyId,
        decimal? buyPrice,
        int count,
        DateTime purchaseDate)
    {
        CurrencyId = currencyId;
        BuyPrice = buyPrice;
        Count = count.AgainstLessOrEqual(
            0,
            () => new InvalidInputException("sale.fact.content.detail.count.required"));
        PurchaseDate = purchaseDate;
    }

    public override int GetId() => Id;
}
