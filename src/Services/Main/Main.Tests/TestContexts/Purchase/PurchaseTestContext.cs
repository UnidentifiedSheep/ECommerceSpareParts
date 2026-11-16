using Main.Entities.Product;
using Main.Entities.Product.Product;
using Main.Entities.Product.Stock;
using Main.Entities.Storage;
using Main.Entities.Storage.Storage;
using Main.Entities.User;
using Persistence.Context;
using Tests.Abstractions;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Purchase;
using Tests.DataBuilders.Storage;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.Interfaces;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;
using DomainPurchase = Main.Entities.Purchase.Purchase.Purchase;

namespace Tests.TestContexts.Purchase;

public class PurchaseTestContext(
	UnifiedDbContext context,
	UserContextTestContext userContext,
	ProductTestContext productContext,
	CurrencyTestContext currencyContext,
	StorageTestContext storageContext)
	: TestContextBase<UnifiedDbContext>(context), IDependentTestContext
{
	public User Supplier { get; private set; } = null!;

	public Product Product { get; private set; } = null!;

	public StorageContent StorageContent { get; private set; } = null!;

	public DomainPurchase Purchase { get; private set; } = null!;

	public static Type[] DependsOn { get; } =
	[
		typeof(ProductTestContext), typeof(CurrencyRatesTestContext), typeof(StorageTestContext)
	];

	public override async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		var currencyId = currencyContext.Currencies[0].Id;
		Product = productContext.Products.First();
		Supplier = await new SupplierUserBuilder(Faker).BuildAndAddToDb(DbContext);

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
			.WithStorageCode(storageContext.Storages.Last().Code)
			.WithProductIds(Product.Id)
			.WithCurrencyId(currencyId)
			.WithCount(2)
			.WithBuyPrice(10m)
			.BuildAndAddToDb(DbContext);

		await DbContext.AddAsync(
			ProductStock.Create(
				StorageContent.StorageCode,
				Product.Id,
				StorageContent.Count),
			cancellationToken);

		var purchaseContent = new PurchaseContentBuilder(Faker)
			.WithProductId(Product.Id)
			.WithCount(2)
			.WithPrice(10m)
			.WithStorageContentId(StorageContent.Id)
			.Build();

		Purchase = await new PurchaseBuilder(Faker)
			.WithSupplierUserId(Supplier.Id)
			.WithSupplierOrganizationId(Supplier.Id)
			.WithCurrencyId(currencyId)
			.WithTransactionId(transaction.Id)
			.WithStorage(storageContext.Storages.Last().Code)
			.WithContent(purchaseContent)
			.BuildAndAddToDb(DbContext);
	}
}
