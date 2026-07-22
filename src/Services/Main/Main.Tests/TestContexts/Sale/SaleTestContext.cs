using Main.Entities.Balance;
using Main.Entities.Organization;
using Main.Entities.Product;
using Main.Entities.Storage;
using Main.Entities.User;
using Main.Enums;
using Main.Enums.Balances;
using Main.Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Sale;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.Interfaces;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;
using DomainSale = Main.Entities.Sale.Sale;

namespace Tests.TestContexts.Sale;

public class SaleTestContext(
    DContext context,
    UserContextTestContext userContext,
    ProductTestContext productContext,
    StorageContentTestContext storageContentContext,
    CurrencyTestContext currencyContext
) : TestContextBase<DContext>(context), IDependentTestContext
{
    public User Buyer { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public StorageContent StorageContent { get; private set; } = null!;
    public DomainSale Sale { get; private set; } = null!;
    public Transaction Transaction { get; private set; } = null!;
    public OrganizationBalance SenderBalance { get; private set; } = null!;
    public OrganizationBalance ReceiverBalance { get; private set; } = null!;
    public int SoldCount { get; private set; }

    public static Type[] DependsOn { get; } =
    [
        typeof(ProductTestContext),
        typeof(CurrencyRatesTestContext),
        typeof(StorageContentTestContext)
    ];

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var currencyId = currencyContext.Currencies[0].Id;
        Buyer = await new MemberUserBuilder(Faker).BuildAndAddToDb(DbContext);
        var buyerProfile = OrganizationFinancialProfile.Create(Buyer.Id);
        StorageContent = storageContentContext.StorageContents.First(x => x.Count > 0);
        Product = productContext.Products.Single(x => x.Id == StorageContent.ProductId);
        SoldCount = 1;

        ReceiverBalance = new UserBalanceBuilder(Faker)
            .WithUserId(Buyer.Id)
            .WithCurrencyId(currencyId)
            .Build();
        ReceiverBalance.IncrementBalance(10_000m);
        SenderBalance = new UserBalanceBuilder(Faker)
            .WithUserId(userContext.SystemUser.Id)
            .WithCurrencyId(currencyId)
            .Build();

        SenderBalance.IncrementBalance(20m);

        Transaction = new TransactionBuilder(Faker)
            .WithSenderId(SenderBalance.OrganizationId)
            .WithReceiverId(ReceiverBalance.OrganizationId)
            .WithCurrencyId(currencyId)
            .WithAmount(20m)
            .WithBalances(SenderBalance, ReceiverBalance)
            .WithSourceType(TransactionSourceType.Sale)
            .Completed()
            .Applied()
            .Build();

        await DbContext.AddRangeAsync(
            Transaction,
            SenderBalance,
            ReceiverBalance,
            buyerProfile);
        await DbContext.SaveChangesAsync(cancellationToken);

        var saleContent = new SaleContentBuilder(Faker)
            .WithProductId(Product.Id)
            .WithStorageContentIds([StorageContent.Id])
            .WithCurrencyId(currencyId)
            .WithCount(SoldCount)
            .WithDetailsCount(1)
            .Build();

        Sale = new SaleBuilder(Faker)
            .WithCurrencyId(currencyId)
            .WithContents([saleContent])
            .WithUserId(Buyer.Id)
            .WithOrganizationId(Buyer.Id)
            .WithStorageName(StorageContent.StorageName)
            .WithTransactionId(Transaction.Id)
            .Completed()
            .Build();

        StorageContent.IncreaseCount(-SoldCount, StorageMovementType.Sale);
        Product.IncreaseStock(-SoldCount);

        await DbContext.AddAsync(Sale, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
