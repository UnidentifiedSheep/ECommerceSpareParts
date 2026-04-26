using Domain;
using Domain.Extensions;

namespace Main.Entities.Sale;

public class SaleContentDetail : Entity<SaleContentDetail, int>
{
    public int Id { get; private set; }
    public int SaleContentId { get; private set; }
    public int? StorageContentId { get; private set; }
    public string Storage { get; private set; } = null!;
    public int CurrencyId { get; private set; }
    public decimal BuyPrice { get; private set; }
    public int Count { get; private set; }
    public DateTime PurchaseDatetime { get; private set; }
    
    private SaleContentDetail() {}

    private SaleContentDetail(
        int? storageContentId, 
        int currencyId, 
        decimal buyPrice, 
        int count, 
        DateTime purchaseDate)
    {
        PurchaseDatetime = purchaseDate;
        StorageContentId = storageContentId;
        CurrencyId = currencyId;
        SetPrice(buyPrice);
        SetCount(count);
    }

    public static SaleContentDetail Create(
        int? storageContentId,
        int currencyId,
        decimal buyPrice,
        int count,
        DateTime purchaseDate)
    {
        return new SaleContentDetail(storageContentId, currencyId, buyPrice, count, purchaseDate);
    }

    private void SetPrice(decimal price)
    {
        BuyPrice = price.AgainstLessOrEqual(
            min: 0,
            exceptionFactory: () => new InvalidOperationException("Price must be greater than zero."));
    }

    private void SetCount(int count)
    {
        Count = count.AgainstLessOrEqual(
            min: 0,
            exceptionFactory: () => new InvalidOperationException("Count must be greater than zero."));
    }
    
    public override int GetId() => Id;
}