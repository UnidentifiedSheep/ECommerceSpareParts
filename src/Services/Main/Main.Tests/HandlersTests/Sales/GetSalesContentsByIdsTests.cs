using Enums;
using FluentAssertions;
using Main.Entities.Balance.Balance;
using Main.Entities.Sale.Sale;
using Main.Modules.Sale.Application.Queries.Sales;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Sale;
using Tests.TestContexts.Sale;

namespace Tests.HandlersTests.Sales;

public sealed class GetSalesContentsByIdsTests(
	ReadScenarioFixture<GetSalesContentsByIdsTests.SalesContentsByIdsScenario> fixture)
	: ReadScenarioTest<GetSalesContentsByIdsTests.SalesContentsByIdsScenario>(fixture)
{
	[Fact]
	public async Task Batch_ReturnsOnlyRequestedContentsGroupedBySaleWithoutDuplicates()
	{
		var first = Scenario.Get<SaleTestContext>().Sale;
		var second = Scenario.Second;

		var result = await Mediator.Send(
			new GetSalesContentsByIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]),
			CancellationToken);

		result.Contents.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
		result.Contents[first.Id]
			.Select(content => content.Id)
			.Should()
			.Equal(first.Contents.Select(content => content.Id).Order());
		result.Contents[second.Id]
			.Select(content => content.Id)
			.Should()
			.Equal(second.Contents.Select(content => content.Id).Order());
	}

	[Fact]
	public async Task EmptySale_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetSalesContentsByIdsQuery(Scenario.Empty.Id), CancellationToken);

		result.Contents.Should().BeEmpty();
	}

	[Fact]
	public async Task MissingSale_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetSalesContentsByIdsQuery(Guid.NewGuid()), CancellationToken);

		result.Contents.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(
			new GetSalesContentsByIdsQuery(Array.Empty<Guid>()),
			CancellationToken);

		result.Contents.Should().BeEmpty();
	}

	public sealed class SalesContentsByIdsScenario : ReadScenario
	{
		private Transaction[] _transactions = [];

		public SalesContentsByIdsScenario() => RegisterBasicContext<SaleTestContext>();

		public Sale Second { get; private set; } = null!;

		public Sale Empty { get; private set; } = null!;

		protected override async Task PrepareAsync()
		{
			var context = Get<SaleTestContext>();
			_transactions = Enumerable.Range(0, 3)
				.Select(_ => new TransactionBuilder(Faker)
					.WithSenderId(context.Transaction.SenderId)
					.WithReceiverId(context.Transaction.ReceiverId)
					.WithCurrencyId(context.Sale.CurrencyId)
					.WithAmount(20m)
					.WithSourceType(TransactionSourceType.Sale)
					.Build())
				.ToArray();
			Context.AddRange(_transactions);
			await Context.SaveChangesAsync(CancellationToken);
		}

		protected override IEnumerable<object> Seed()
		{
			var context = Get<SaleTestContext>();
			Second = CreateSale(context, _transactions[0], true);
			var other = CreateSale(context, _transactions[1], true);
			Empty = CreateSale(context, _transactions[2], false);
			return [Second, other, Empty];
		}

		private Sale CreateSale(SaleTestContext context, Transaction transaction, bool withContents)
		{
			var contents = withContents
				? Enumerable.Range(0, 2)
					.Select(_ => new SaleContentBuilder(Faker)
						.WithProductId(context.Product.Id)
						.WithCurrencyId(context.Sale.CurrencyId)
						.WithStorageContentIds([context.StorageContent.Id])
						.WithCount(1)
						.WithDetailsCount(1)
						.Build())
					.ToArray()
				: Array.Empty<SaleContent>();
			return new SaleBuilder(Faker)
				.WithUserId(context.Buyer.Id)
				.WithOrganizationId(context.Sale.OrganizationId)
				.WithTransactionId(transaction.Id)
				.WithCurrencyId(context.Sale.CurrencyId)
				.WithStorageCode(context.Sale.StorageCode)
				.WithSaleDate(DateTime.UtcNow)
				.WithContents(contents)
				.Build();
		}
	}
}
