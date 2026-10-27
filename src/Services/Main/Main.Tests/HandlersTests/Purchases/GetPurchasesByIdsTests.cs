using FluentAssertions;
using Main.Application.Handlers.Purchases;
using Main.Entities.Purchase;
using Main.Enums.Balances;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Purchase;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Purchase;

namespace Tests.HandlersTests.Purchases;

public sealed class GetPurchasesByIdsTests : IntegrationTest
{
	public GetPurchasesByIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<PurchaseTestContext>();
	}

	private PurchaseTestContext TestContext => GetContext<PurchaseTestContext>();

	[Fact]
	public async Task Batch_ReturnsOnlyRequestedPurchasesWithoutDuplicates()
	{
		var first = await CreatePurchase();
		var second = await CreatePurchase();

		var result = await Mediator.Send(
			new GetPurchasesByIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]), CancellationToken);

		result.Purchases.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
	}

	[Fact]
	public async Task SinglePurchase_IsReturnedById()
	{
		var id = TestContext.Purchase.Id;

		var result = await Mediator.Send(new GetPurchasesByIdsQuery(id), CancellationToken);

		result.Purchases.Keys.Should().Equal(id);
	}

	[Fact]
	public async Task MissingPurchase_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetPurchasesByIdsQuery(Guid.NewGuid()), CancellationToken);

		result.Purchases.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetPurchasesByIdsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Purchases.Should().BeEmpty();
	}

	private async Task<Purchase> CreatePurchase()
	{
		var seed = TestContext.Purchase;
		var transaction = await new TransactionBuilder(Faker)
			.WithSenderId(TestContext.Purchase.SupplierOrganizationId)
			.WithReceiverId(GetContext<UserContextTestContext>().SystemUser.Id)
			.WithCurrencyId(seed.CurrencyId)
			.WithAmount(20m)
			.WithSourceType(TransactionSourceType.Purchase)
			.BuildAndAddToDb(Context);

		return await new PurchaseBuilder(Faker)
			.WithSupplierUserId(TestContext.Supplier.Id)
			.WithSupplierOrganizationId(seed.SupplierOrganizationId)
			.WithTransactionId(transaction.Id)
			.WithCurrencyId(seed.CurrencyId)
			.WithStorage(seed.Storage)
			.WithPurchaseDateTime(DateTime.UtcNow)
			.BuildAndAddToDb(Context);
	}
}
