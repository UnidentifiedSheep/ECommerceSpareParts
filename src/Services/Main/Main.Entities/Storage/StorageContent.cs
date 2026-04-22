using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;

namespace Main.Entities.Storage;

public class StorageContent : AuditableEntity<StorageContent, int>
{
    [Validate]
    public int Id { get; private set; }

    public string StorageName { get; private set; } = null!;

    public int ProductId { get; private set; }

    public int Count { get; private set; }

    public decimal BuyPrice { get; private set; }

    public int CurrencyId { get; private set; }

    public DateTime PurchaseDatetime { get; private set; }

    public Currency.Currency Currency { get; private set; } = null!;
    
    private StorageContent() {}

    private StorageContent(
        string storageName,
        int productId,
        int count,
        decimal buyPrice,
        int currencyId,
        DateTime purchaseDatetime)
    {
        StorageName = storageName;
        ProductId = productId;
        CurrencyId = currencyId;
        PurchaseDatetime = purchaseDatetime;
        SetCount(count);
        SetBuyPrice(buyPrice);
    }

    public static StorageContent Create(
        string storageName,
        int productId,
        int count,
        decimal buyPrice,
        int currencyId,
        DateTime purchaseDatetime)
    {
        return new StorageContent(storageName, productId, count, buyPrice, currencyId, purchaseDatetime);
    }

    public void SetCount(int count)
    {
        Count = count
            .AgainstNegative(() => new InvalidOperationException("Count must be greater than or equal to zero."));
    }

    public void SetBuyPrice(decimal buyPrice)
    {
        BuyPrice = buyPrice
            .AgainstTooManyDecimalPlaces(
                maxDecimals: 2,
                exceptionFactory: () => new InvalidOperationException("Buy price must have maximum 2 decimal places."))
            .AgainstTooSmall(
                min: 0.001m,
                exceptionFactory: () => new InvalidOperationException("Buy price must be grater then 0."));
    }
    
    public override int GetId() => Id;
}