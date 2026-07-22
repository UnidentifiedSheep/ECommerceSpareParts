using Bogus;
using Main.Entities.Storage;
using Tests.Abstractions;
using DomainPurchase = Main.Entities.Purchase.Purchase;
using PurchaseContent = Main.Entities.Purchase.PurchaseContent;

namespace Tests.DataBuilders.Purchase;

public class PurchaseBuilder(Faker faker) : BuilderBase<DomainPurchase>(faker)
{
    private readonly List<PurchaseContent> _contents = [];

    public Guid? SupplierUserId { get; private set; }
    public Guid? SupplierOrganizationId { get; private set; }
    public int? CurrencyId { get; private set; }
    public Guid? TransactionId { get; private set; }
    public string? Storage { get; private set; }
    public DateTime? PurchaseDateTime { get; private set; }
    public StorageRoute? LogisticRoute { get; private set; }
    public Guid? LogisticTransactionId { get; private set; }
    public bool LogisticMinimumPriceApplied { get; private set; }

    public PurchaseBuilder WithSupplierUserId(Guid supplierUserId)
    {
        SupplierUserId = supplierUserId;
        return this;
    }

    public PurchaseBuilder WithSupplierOrganizationId(Guid supplierOrganizationId)
    {
        SupplierOrganizationId = supplierOrganizationId;
        return this;
    }

    public PurchaseBuilder WithCurrencyId(int currencyId)
    {
        CurrencyId = currencyId;
        return this;
    }

    public PurchaseBuilder WithTransactionId(Guid transactionId)
    {
        TransactionId = transactionId;
        return this;
    }

    public PurchaseBuilder WithStorage(string storage)
    {
        Storage = storage;
        return this;
    }

    public PurchaseBuilder WithPurchaseDateTime(DateTime purchaseDateTime)
    {
        PurchaseDateTime = purchaseDateTime;
        return this;
    }

    public PurchaseBuilder WithContent(PurchaseContent content)
    {
        _contents.Add(content);
        return this;
    }

    public PurchaseBuilder WithLogistic(
        StorageRoute route,
        Guid? transactionId = null,
        bool minimumPriceApplied = false)
    {
        LogisticRoute = route;
        LogisticTransactionId = transactionId;
        LogisticMinimumPriceApplied = minimumPriceApplied;
        return this;
    }

    public override DomainPurchase Build()
    {
        var supplierUserId = SupplierUserId ?? Guid.NewGuid();
        var purchase = DomainPurchase.Create(
            supplierUserId,
            SupplierOrganizationId ?? supplierUserId,
            CurrencyId ?? Faker.Random.Int(1),
            TransactionId ?? Guid.NewGuid(),
            Storage ?? Faker.Lorem.Word(),
            PurchaseDateTime ?? DateTime.UtcNow);

        foreach (var content in _contents) purchase.AddContent(content);

        if (LogisticRoute is not null)
            purchase.SetPurchaseLogistic(
                LogisticRoute.Id,
                LogisticRoute.CurrencyId,
                LogisticRoute.PricingModel,
                LogisticRoute.RouteType,
                LogisticRoute.PriceKg,
                LogisticRoute.PricePerM3,
                LogisticRoute.PricePerOrder,
                LogisticRoute.MinimumPrice,
                LogisticTransactionId,
                LogisticMinimumPriceApplied);

        return purchase;
    }
}
