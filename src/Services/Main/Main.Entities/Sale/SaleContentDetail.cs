using System.Linq.Expressions;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Sale;

public class SaleContentDetail : Entity<SaleContentDetail, int>, ILinqEntity<SaleContentDetail, int>
{
    private SaleContentDetail()
    {
    }

    private SaleContentDetail(
        int storageContentId,
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

    public int Id { get; private set; }
    public int SaleContentId { get; private set; }
    public int StorageContentId { get; private set; }
    public string Storage { get; private set; } = null!;
    public int CurrencyId { get; private set; }
    public decimal BuyPrice { get; private set; }
    public int Count { get; private set; }
    public DateTime PurchaseDatetime { get; private set; }

    public static Expression<Func<SaleContentDetail, int>> GetKeySelector()
    {
        return x => x.Id;
    }

    public static Expression<Func<SaleContentDetail, bool>> GetEqualityExpression(int key)
    {
        return x => x.Id == key;
    }

    public static SaleContentDetail Create(
        int storageContentId,
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
            0,
            () => new InvalidOperationException("Price must be greater than zero."));
    }

    private void SetCount(int count)
    {
        Count = count.AgainstLessOrEqual(
            0,
            () => new InvalidOperationException("Count must be greater than zero."));
    }

    public override int GetId()
    {
        return Id;
    }
}
