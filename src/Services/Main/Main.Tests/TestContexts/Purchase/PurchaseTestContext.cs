using Main.Entities.Product;
using Main.Entities.Storage;
using Main.Entities.User;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Purchase;
using Tests.DataBuilders.Storage;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.Interfaces;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;
using DomainPurchase = Main.Entities.Purchase.Purchase;

namespace Tests.TestContexts.Purchase;

public class PurchaseTestContext(
    DContext context,
    UserContextTestContext userContext,
    ProductTestContext productContext,
    CurrencyTestContext currencyContext,
    StorageRouteTestContext storageRouteContext
)
    : TestContextBase<DContext>(context), IDependentTestContext
{
    public User Supplier { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public StorageContent StorageContent { get; private set; } = null!;
    public DomainPurchase Purchase { get; private set; } = null!;

    public static Type[] DependsOn { get; } =
    [
        typeof(ProductTestContext),
        typeof(CurrencyRatesTestContext),
        typeof(StorageRouteTestContext)
    ];

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var currencyId = currencyContext.Currencies[0].Id;
        Product = productContext.Products.First();
        Supplier = await new SupplierUserBuilder(Faker)
            .BuildAndAddToDb(DbContext);

        var senderBalance = new UserBalanceBuilder(Faker)
            .WithUserId(Supplier.Id)
            .WithCurrencyId(currencyId)
            .Build();
        var receiverBalance = new UserBalanceBuilder(Faker)
            .WithUserId(userContext.SystemUser.Id)
            .WithCurrencyId(currencyId)
            .Build();
        senderBalance.IncrementBalance(20m);

        var transaction = new TransactionBuilder(Faker)
            .WithSenderId(Supplier.Id)
            .WithReceiverId(userContext.SystemUser.Id)
            .WithCurrencyId(currencyId)
            .WithAmount(20m)
            .WithBalances(senderBalance, receiverBalance)
            .Completed()
            .Applied()
            .Build();

        await DbContext.AddRangeAsync(
            transaction,
            senderBalance,
            receiverBalance);
        await DbContext.SaveChangesAsync(cancellationToken);

        StorageContent = await new StorageContentBuilder(Faker)
            .WithStorageName(storageRouteContext.ActiveRoute.ToStorageName)
            .WithProductIds(Product.Id)
            .WithCurrencyId(currencyId)
            .WithCount(2)
            .WithBuyPrice(10m)
            .BuildAndAddToDb(DbContext);

        Product.IncreaseStock(StorageContent.Count);

        var purchaseContent = new PurchaseContentBuilder(Faker)
            .WithProductId(Product.Id)
            .WithCount(2)
            .WithPrice(10m)
            .WithStorageContentId(StorageContent.Id)
            .Build();

        Purchase = await new PurchaseBuilder(Faker)
            .WithSupplierId(Supplier.Id)
            .WithCurrencyId(currencyId)
            .WithTransactionId(transaction.Id)
            .WithStorage(storageRouteContext.ActiveRoute.ToStorageName)
            .WithContent(purchaseContent)
            .BuildAndAddToDb(DbContext);
    }
}