using Enums;
using FluentAssertions;
using Main.Entities.Balance.Balance;
using Main.Entities.Sale.Sale;
using Main.Modules.Sale.Application.Queries.Sales;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Sale;
using Tests.TestContexts.Sale;

namespace Tests.HandlersTests.Sales;

public sealed class GetSalesByIdsTests(ReadScenarioFixture<GetSalesByIdsTests.SalesByIdsScenario> fixture)
	: ReadScenarioTest<GetSalesByIdsTests.SalesByIdsScenario>(fixture)
{
	[Fact]
	public async Task Batch_ReturnsOnlyRequestedSalesWithoutDuplicates()
	{
		var first = Scenario.First;
		var second = Scenario.Second;

		var result = await Mediator.Send(
			new GetSalesByIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]),
			CancellationToken);

		result.Sales.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
	}

	[Fact]
	public async Task SingleSale_IsReturnedById()
	{
		var id = Scenario.First.Id;

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
		var id = Scenario.Deleted.Id;

		var result = await Mediator.Send(new GetSalesByIdsQuery(id), CancellationToken);

		result.Sales.Keys.Should().Equal(id);
	}

	public sealed class SalesByIdsScenario : ReadScenario
	{
		public SalesByIdsScenario() => RegisterBasicContext<SaleTestContext>();

		public Sale First { get; private set; } = null!;

		public Sale Second { get; private set; } = null!;

		public Sale Deleted { get; private set; } = null!;

		private Transaction[] _transactions = [];

		protected override async Task PrepareAsync()
		{
			var context = Get<SaleTestContext>();
			_transactions =
			[
				CreateTransaction(context),
				CreateTransaction(context)
			];
			Context.AddRange(_transactions);
			await Context.SaveChangesAsync(CancellationToken);
		}

		protected override IEnumerable<object> Seed()
		{
			var context = Get<SaleTestContext>();
			Deleted = context.Sale;
			Deleted.Delete();
			First = CreateSale(context, _transactions[0]);
			Second = CreateSale(context, _transactions[1]);
			return [First, Second];
		}

		private Transaction CreateTransaction(SaleTestContext context)
			=> new TransactionBuilder(Faker)
				.WithSenderId(context.Transaction.SenderId)
				.WithReceiverId(context.Transaction.ReceiverId)
				.WithCurrencyId(context.Sale.CurrencyId)
				.WithAmount(20m)
				.WithSourceType(TransactionSourceType.Sale)
				.Build();

		private Sale CreateSale(SaleTestContext context, Transaction transaction)
			=> new SaleBuilder(Faker)
				.WithUserId(context.Buyer.Id)
				.WithOrganizationId(context.Sale.OrganizationId)
				.WithTransactionId(transaction.Id)
				.WithCurrencyId(context.Sale.CurrencyId)
				.WithStorageCode(context.Sale.StorageCode)
				.WithSaleDate(DateTime.UtcNow)
				.Build();
	}
}
