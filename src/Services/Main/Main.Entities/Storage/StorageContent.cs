using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Main.Entities.DomainEvents.StorageContent;
using Main.Enums;

namespace Main.Entities.Storage;

public class StorageContent : AuditableEntity<StorageContent, int>, ILinqEntity<StorageContent, int>,
    IVersionable<uint>
{
    private StorageContent() { }

    private StorageContent(
        string storageName,
        int productId,
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

    public static Expression<Func<StorageContent, int>> GetKeySelector() { return x => x.Id; }

    public static Expression<Func<StorageContent, bool>> GetEqualityExpression(int key)
    {
        return x => x.Id == key;
    }

    public uint RowVersion { get; private set; }

    public static StorageContent Create(
        string storageName,
        int productId,
        decimal buyPrice,
        int currencyId,
        decimal buyPriceInBaseCurrency,
        int buyPriceInBaseCurrencyId,
        DateTime purchaseDatetime)
    {
        return new StorageContent(
            storageName,
            productId,
            buyPrice,
            currencyId,
            buyPriceInBaseCurrency,
            buyPriceInBaseCurrencyId,
            purchaseDatetime);
    }

    public void SetCount(int count, StorageMovementType movementType)
    {
        var newCount = count
            .AgainstNegative(() =>
                new InvalidOperationException("Count must be greater than or equal to zero."));
        
        if (Count == newCount)
            return;

        AddDomainEvent(new StorageContentCountUpdatedDomainEvent(
            ProductId,
            StorageName,
            CurrencyId,
            newCount,
            BuyPrice,
            movementType,
             newCount - Count));
        
        Count = newCount;
    }

    public void IncreaseCount(int amount, StorageMovementType movementType)
    {
        SetCount(Count + amount, movementType);
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
    public override void OnDeleted()
    {
        AddDomainEvent(new StorageContentCountUpdatedDomainEvent(
            ProductId,
            StorageName,
            CurrencyId,
            0,
            BuyPrice,
            StorageMovementType.StorageContentDeletion,
            -Count));
        AddDomainEvent(new StorageContentUpdatedDomainEvent(this, true));
    }

    public override void OnUpdated()
        => AddDomainEvent(new StorageContentUpdatedDomainEvent(this, false));

    public override void OnCreated() 
        => AddDomainEvent(new StorageContentUpdatedDomainEvent(this, false));

    public void SetCurrencyId(int currencyId) { CurrencyId = currencyId; }

    public void AssignCurrency(Currency.Currency currency)
    {
        CurrencyId = currency.Id;
        Currency = currency;
    }

    public void SetBaseCurrencyId(int baseCurrencyId) { BaseCurrencyId = baseCurrencyId; }

    public void SetPurchaseDate(DateTime purchaseDate) { PurchaseDatetime = purchaseDate; }

    public override int GetId() { return Id; }
}