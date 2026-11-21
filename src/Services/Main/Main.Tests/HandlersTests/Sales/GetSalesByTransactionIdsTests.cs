using Enums;
using FluentAssertions;
using Main.Entities.Balance.Balance;
using Main.Entities.Sale.Sale;
using Main.Modules.Sale.Application.Queries.Sales;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Sale;
using Tests.TestContexts.Sale;

namespace Tests.HandlersTests.Sales;

public sealed class GetSalesByTransactionIdsTests(
	ReadScenarioFixture<GetSalesByTransactionIdsTests.SalesByTransactionIdsScenario> fixture)
	: ReadScenarioTest<GetSalesByTransactionIdsTests.SalesByTransactionIdsScenario>(fixture)
{
	[Fact]
	public async Task Batch_ReturnsOnlyRequestedSalesKeyedByTransactionWithoutDuplicates()
	{
		var first = Scenario.First;
		var second = Scenario.Second;

		var result = await Mediator.Send(
			new GetSalesByTransactionIdsQuery(
				[first.TransactionId, second.TransactionId, first.TransactionId, Guid.NewGuid()]),
			CancellationToken);

		result.Sales.Keys.Should().BeEquivalentTo([first.TransactionId, second.TransactionId]);
		result.Sales[first.TransactionId].Id.Should().Be(first.Id);
		result.Sales[second.TransactionId].Id.Should().Be(second.Id);
	}

	[Fact]
	public async Task TransactionWithoutSale_IsOmitted()
	{
		var result = await Mediator.Send(
			new GetSalesByTransactionIdsQuery(Scenario.ManualTransaction.Id),
			CancellationToken);

		result.Sales.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(
			new GetSalesByTransactionIdsQuery(Array.Empty<Guid>()),
			CancellationToken);

		result.Sales.Should().BeEmpty();
	}

	[Fact]
	public async Task ReplacedTransaction_IsOmittedAndCurrentTransactionReturnsSale()
	{
		var sale = Scenario.Replaced;

		var result = await Mediator.Send(
			new GetSalesByTransactionIdsQuery([Scenario.OldTransactionId, sale.TransactionId]),
			CancellationToken);

		result.Sales.Keys.Should().Equal(sale.TransactionId);
		result.Sales[sale.TransactionId].Id.Should().Be(sale.Id);
	}

	[Fact]
	public async Task DeletedSale_RemainsLinkedToItsTransaction()
	{
		var sale = Scenario.Deleted;

		var result = await Mediator.Send(
			new GetSalesByTransactionIdsQuery(sale.TransactionId),
			CancellationToken);

		result.Sales.Keys.Should().Equal(sale.TransactionId);
		result.Sales[sale.TransactionId].Id.Should().Be(sale.Id);
	}

	public sealed class SalesByTransactionIdsScenario : ReadScenario
	{
		private Transaction[] _saleTransactions = [];

		public SalesByTransactionIdsScenario() => RegisterBasicContext<SaleTestContext>();

		public Sale First { get; private set; } = null!;

		public Sale Second { get; private set; } = null!;

		public Sale Replaced { get; private set; } = null!;

		public Sale Deleted { get; private set; } = null!;

		public Guid OldTransactionId { get; private set; }

		public Transaction ManualTransaction { get; private set; } = null!;

		protected override async Task PrepareAsync()
		{
			var context = Get<SaleTestContext>();
			_saleTransactions = Enumerable.Range(0, 4)
				.Select(_ => CreateTransaction(context, TransactionSourceType.Sale))
				.ToArray();
			ManualTransaction = CreateTransaction(context, TransactionSourceType.Manual);
			Context.AddRange(_saleTransactions);
			Context.Add(ManualTransaction);
			await Context.SaveChangesAsync(CancellationToken);
		}

		protected override IEnumerable<object> Seed()
		{
			var context = Get<SaleTestContext>();
			First = context.Sale;
			Second = CreateSale(context, _saleTransactions[0]);
			var other = CreateSale(context, _saleTransactions[1]);
			Replaced = CreateSale(context, _saleTransactions[2]);
			OldTransactionId = Replaced.TransactionId;
			Replaced.SetTransactionId(_saleTransactions[3].Id);
			Deleted = context.Sale;
			Deleted.Delete();
			return [Second, other, Replaced];
		}

		private Sale CreateSale(SaleTestContext context, Transaction transaction)
			=> new SaleBuilder(Faker)
				.WithUserId(context.Buyer.Id)
				.WithOrganizationId(context.Sale.OrganizationId)
				.WithTransactionId(transaction.Id)
				.WithCurrencyId(context.Sale.CurrencyId)
				.WithStorageCode(context.Sale.StorageCode)
				.WithSaleDate(DateTime.UtcNow)
				.Build();

		private Transaction CreateTransaction(SaleTestContext context, TransactionSourceType sourceType)
			=> new TransactionBuilder(Faker)
				.WithSenderId(context.Transaction.SenderId)
				.WithReceiverId(context.Transaction.ReceiverId)
				.WithCurrencyId(context.Sale.CurrencyId)
				.WithSourceType(sourceType)
				.WithAmount(20m)
				.Build();
	}
}
