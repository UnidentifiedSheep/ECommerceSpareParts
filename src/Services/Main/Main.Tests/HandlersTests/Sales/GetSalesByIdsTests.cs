using FluentAssertions;
using Main.Application.Handlers.Sales;
using Main.Entities.Sale;
using Main.Enums.Balances;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Sale;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Sale;

namespace Tests.HandlersTests.Sales;

public sealed class GetSalesByIdsTests : IntegrationTest
{
	public GetSalesByIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<SaleTestContext>();
	}

	private SaleTestContext TestContext => GetContext<SaleTestContext>();

	[Fact]
	public async Task Batch_ReturnsOnlyRequestedSalesWithoutDuplicates()
	{
		var first = await CreateSale();
		var second = await CreateSale();

		var result = await Mediator.Send(
			new GetSalesByIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]), CancellationToken);

		result.Sales.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
	}

	[Fact]
	public async Task SingleSale_IsReturnedById()
	{
		var id = TestContext.Sale.Id;

		var result = await Mediator.Send(new GetSalesByIdsQuery(id), CancellationToken);

		result.Sales.Keys.Should().Equal(id);
	}

	[Fact]
	public async Task MissingSale_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetSalesByIdsQuery(Guid.NewGuid()), CancellationToken);

		result.Sales.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetSalesByIdsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Sales.Should().BeEmpty();
	}

	[Fact]
	public async Task DeletedSale_IsReturnedById()
	{
		var sale = TestContext.Sale;
		sale.Delete();
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(new GetSalesByIdsQuery(sale.Id), CancellationToken);

		result.Sales.Keys.Should().Equal(sale.Id);
	}

	private async Task<Sale> CreateSale()
	{
		var seed = TestContext.Sale;
		var transaction = await new TransactionBuilder(Faker)
			.WithSenderId(TestContext.Transaction.SenderId)
			.WithReceiverId(TestContext.Transaction.ReceiverId)
			.WithCurrencyId(seed.CurrencyId)
			.WithAmount(20m)
			.WithSourceType(TransactionSourceType.Sale)
			.BuildAndAddToDb(Context);

		return await new SaleBuilder(Faker)
			.WithUserId(TestContext.Buyer.Id)
			.WithOrganizationId(seed.OrganizationId)
			.WithTransactionId(transaction.Id)
			.WithCurrencyId(seed.CurrencyId)
			.WithStorageCode(seed.StorageCode)
			.WithSaleDate(DateTime.UtcNow)
			.BuildAndAddToDb(Context);
	}
}
