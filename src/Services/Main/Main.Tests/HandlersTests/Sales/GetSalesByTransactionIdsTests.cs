using FluentAssertions;
using Main.Application.Handlers.Sales;
using Main.Entities.Balance;
using Main.Entities.Sale;
using Main.Enums.Balances;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Sale;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Sale;

namespace Tests.HandlersTests.Sales;

public sealed class GetSalesByTransactionIdsTests : IntegrationTest
{
	public GetSalesByTransactionIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<SaleTestContext>();
	}

	private SaleTestContext TestContext => GetContext<SaleTestContext>();

	[Fact]
	public async Task Batch_ReturnsOnlyRequestedSalesKeyedByTransactionWithoutDuplicates()
	{
		var first = TestContext.Sale;
		var second = await CreateSale();
		await CreateSale();

		var result = await Mediator.Send(new GetSalesByTransactionIdsQuery(
			[first.TransactionId, second.TransactionId, first.TransactionId, Guid.NewGuid()]), CancellationToken);

		result.Sales.Keys.Should().BeEquivalentTo([first.TransactionId, second.TransactionId]);
		result.Sales[first.TransactionId].Id.Should().Be(first.Id);
		result.Sales[second.TransactionId].Id.Should().Be(second.Id);
	}

	[Fact]
	public async Task TransactionWithoutSale_IsOmitted()
	{
		var transaction = await CreateTransaction(TransactionSourceType.Manual);

		var result = await Mediator.Send(new GetSalesByTransactionIdsQuery(transaction.Id), CancellationToken);

		result.Sales.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetSalesByTransactionIdsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Sales.Should().BeEmpty();
	}

	[Fact]
	public async Task ReplacedTransaction_IsOmittedAndCurrentTransactionReturnsSale()
	{
		var sale = TestContext.Sale;
		var oldTransactionId = sale.TransactionId;
		var transaction = await CreateTransaction(TransactionSourceType.Sale);
		sale.SetTransactionId(transaction.Id);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(
			new GetSalesByTransactionIdsQuery([oldTransactionId, transaction.Id]), CancellationToken);

		result.Sales.Keys.Should().Equal(transaction.Id);
		result.Sales[transaction.Id].Id.Should().Be(sale.Id);
	}

	[Fact]
	public async Task DeletedSale_RemainsLinkedToItsTransaction()
	{
		var sale = TestContext.Sale;
		sale.Delete();
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(new GetSalesByTransactionIdsQuery(sale.TransactionId), CancellationToken);

		result.Sales.Keys.Should().Equal(sale.TransactionId);
		result.Sales[sale.TransactionId].Id.Should().Be(sale.Id);
	}

	private async Task<Sale> CreateSale()
	{
		var transaction = await CreateTransaction(TransactionSourceType.Sale);
		var seed = TestContext.Sale;
		return await new SaleBuilder(Faker)
			.WithUserId(TestContext.Buyer.Id)
			.WithOrganizationId(seed.OrganizationId)
			.WithTransactionId(transaction.Id)
			.WithCurrencyId(seed.CurrencyId)
			.WithStorageCode(seed.StorageCode)
			.WithSaleDate(DateTime.UtcNow)
			.BuildAndAddToDb(Context);
	}

	private Task<Transaction> CreateTransaction(TransactionSourceType sourceType) =>
		new TransactionBuilder(Faker)
			.WithSenderId(TestContext.Transaction.SenderId)
			.WithReceiverId(TestContext.Transaction.ReceiverId)
			.WithCurrencyId(TestContext.Sale.CurrencyId)
			.WithSourceType(sourceType)
			.WithAmount(20m)
			.BuildAndAddToDb(Context);
}
