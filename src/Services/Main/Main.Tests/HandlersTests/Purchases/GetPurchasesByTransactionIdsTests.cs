using FluentAssertions;
using Main.Application.Handlers.Purchases;
using Main.Entities.Balance;
using Main.Entities.Purchase;
using Main.Enums.Balances;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Purchase;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Purchase;

namespace Tests.HandlersTests.Purchases;

public sealed class GetPurchasesByTransactionIdsTests : IntegrationTest
{
	public GetPurchasesByTransactionIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<PurchaseTestContext>();
	}

	private PurchaseTestContext TestContext => GetContext<PurchaseTestContext>();

	[Fact]
	public async Task Batch_ReturnsOnlyRequestedPurchasesKeyedByTransactionWithoutDuplicates()
	{
		var first = TestContext.Purchase;
		var second = await CreatePurchase();
		await CreatePurchase();

		var result = await Mediator.Send(new GetPurchasesByTransactionIdsQuery(
			[first.TransactionId, second.TransactionId, first.TransactionId, Guid.NewGuid()]), CancellationToken);

		result.Purchases.Keys.Should().BeEquivalentTo([first.TransactionId, second.TransactionId]);
		result.Purchases[first.TransactionId].Id.Should().Be(first.Id);
		result.Purchases[second.TransactionId].Id.Should().Be(second.Id);
	}

	[Fact]
	public async Task TransactionWithoutPurchase_IsOmitted()
	{
		var transaction = await CreateTransaction(TransactionSourceType.Manual);

		var result = await Mediator.Send(new GetPurchasesByTransactionIdsQuery(transaction.Id), CancellationToken);

		result.Purchases.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetPurchasesByTransactionIdsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Purchases.Should().BeEmpty();
	}

	[Fact]
	public async Task ReplacedTransaction_IsOmittedAndCurrentTransactionReturnsPurchase()
	{
		var purchase = TestContext.Purchase;
		var oldTransactionId = purchase.TransactionId;
		var transaction = await CreateTransaction(TransactionSourceType.Purchase);
		purchase.SetTransactionId(transaction.Id);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(
			new GetPurchasesByTransactionIdsQuery([oldTransactionId, transaction.Id]), CancellationToken);

		result.Purchases.Keys.Should().Equal(transaction.Id);
		result.Purchases[transaction.Id].Id.Should().Be(purchase.Id);
	}

	[Fact]
	public async Task DeletedPurchase_IsNoLongerLinkedToItsTransaction()
	{
		var purchase = TestContext.Purchase;
		Context.Remove(purchase);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(new GetPurchasesByTransactionIdsQuery(purchase.TransactionId), CancellationToken);

		result.Purchases.Should().BeEmpty();
	}

	private async Task<Purchase> CreatePurchase()
	{
		var transaction = await CreateTransaction(TransactionSourceType.Purchase);
		var seed = TestContext.Purchase;
		return await new PurchaseBuilder(Faker)
			.WithSupplierUserId(TestContext.Supplier.Id)
			.WithSupplierOrganizationId(seed.SupplierOrganizationId)
			.WithTransactionId(transaction.Id)
			.WithCurrencyId(seed.CurrencyId)
			.WithStorage(seed.Storage)
			.WithPurchaseDateTime(DateTime.UtcNow)
			.BuildAndAddToDb(Context);
	}

	private Task<Transaction> CreateTransaction(TransactionSourceType sourceType) =>
		new TransactionBuilder(Faker)
			.WithSenderId(TestContext.Purchase.SupplierOrganizationId)
			.WithReceiverId(GetContext<UserContextTestContext>().SystemUser.Id)
			.WithCurrencyId(TestContext.Purchase.CurrencyId)
			.WithSourceType(sourceType)
			.WithAmount(20m)
			.BuildAndAddToDb(Context);
}
