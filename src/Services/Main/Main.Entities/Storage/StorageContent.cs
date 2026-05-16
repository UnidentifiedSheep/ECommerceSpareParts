using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;

namespace Main.Entities.Storage;

public class StorageContent : AuditableEntity<StorageContent, int>, ILinqEntity<StorageContent, int>, IVersionable<uint>
{
    private StorageContent()
    {
    }

    private StorageContent(
        string storageName,
        int productId,
        int count,
        decimal buyPrice,
        int currencyId,
        decimal buyPriceInBaseCurrency,
        int buyPriceInBaseCurrencyId,
        DateTime purchaseDatetime)
    {
        StorageName = storageName;
        ProductId = productId;
        PurchaseDatetime = purchaseDatetime;
        SetCurrencyId(currencyId);
        SetBaseCurrencyId(buyPriceInBaseCurrencyId);
        SetCount(count);
        SetBuyPrice(buyPrice, buyPriceInBaseCurrency);
    }

    [Validate]
    public int Id { get; private set; }

    public string StorageName { get; private set; } = null!;

    public int ProductId { get; private set; }

    public int Count { get; private set; }

    public decimal BuyPrice { get; private set; }

    public decimal BuyPriceInBaseCurrency { get; private set; }
    public int BaseCurrencyId { get; private set; }

    public int CurrencyId { get; private set; }

    public DateTime PurchaseDatetime { get; private set; }

    public Currency.Currency Currency { get; private set; } = null!;

    public uint RowVersion { get; private set; }

    public static StorageContent Create(
        string storageName,
        int productId,
        int count,
        decimal buyPrice,
        int currencyId,
        decimal buyPriceInBaseCurrency,
        int buyPriceInBaseCurrencyId,
        DateTime purchaseDatetime)
    {
        return new StorageContent(
            storageName,
            productId,
            count,
            buyPrice,
            currencyId,
            buyPriceInBaseCurrency,
            buyPriceInBaseCurrencyId,
            purchaseDatetime);
    }

    public void SetCount(int count)
    {
        Count = count
            .AgainstNegative(() => new InvalidOperationException("Count must be greater than or equal to zero."));
    }

    public void IncreaseCount(int amount)
    {
        Count = (Count + amount)
            .AgainstNegative(() => new InvalidOperationException("Count must be greater than or equal to zero."));
    }

    public void SetBuyPrice(decimal buyPrice, decimal buyPriceInBaseCurrency)
    {
        buyPrice
            .AgainstTooManyDecimalPlaces(
                2,
                () => new InvalidOperationException("Buy price must have maximum 2 decimal places."))
            .AgainstTooSmall(
                0.001m,
                () => new InvalidOperationException("Buy price must be grater then 0."));

        buyPriceInBaseCurrency
            .AgainstLessOrEqual(
                0,
                () => new InvalidOperationException("Buy price in base currency must be greater then 0."));

        BuyPrice = buyPrice;
        BuyPriceInBaseCurrency = buyPriceInBaseCurrency;
    }

    public void SetCurrencyId(int currencyId)
    {
        CurrencyId = currencyId;
    }

    public void AssignCurrency(Currency.Currency currency)
    {
        CurrencyId = currency.Id;
        Currency = currency;
    }

    public void SetBaseCurrencyId(int baseCurrencyId)
    {
        BaseCurrencyId = baseCurrencyId;
    }

    public void SetPurchaseDate(DateTime purchaseDate)
    {
        PurchaseDatetime = purchaseDate;
    }

    public override int GetId()
    {
        return Id;
    }

    public static Expression<Func<StorageContent, int>> GetKeySelector()
        => x => x.Id;

    public static Expression<Func<StorageContent, bool>> GetEqualityExpression(int key)
        => x => x.Id == key;
}
