using FluentAssertions;
using Main.Entities.Balance.Balance;
using Main.Modules.Balance.Application.Queries.Balance;
using Tests.DataBuilders.Balance;
using Tests.TestContexts.Balance;

namespace Tests.HandlersTests.Balances;

public sealed class GetTransactionsByIdsTests(ReadScenarioFixture<GetTransactionsByIdsTests.TransactionsByIdsScenario> fixture)
	: ReadScenarioTest<GetTransactionsByIdsTests.TransactionsByIdsScenario>(fixture)
{
	[Fact]
	public async Task Batch_ReturnsOnlyRequestedTransactionsWithoutDuplicates()
	{
		var first = Scenario.First;
		var second = Scenario.Second;

		var result = await Mediator.Send(
			new GetTransactionsByIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]),
			CancellationToken);

		result.Transactions.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
	}

	[Fact]
	public async Task SingleTransaction_IsReturnedById()
	{
		var id = Scenario.First.Id;

		var result = await Mediator.Send(new GetTransactionsByIdsQuery(id), CancellationToken);

		result.Transactions.Keys.Should().Equal(id);
	}

	[Fact]
	public async Task MissingTransaction_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(new GetTransactionsByIdsQuery(Guid.NewGuid()), CancellationToken);

		result.Transactions.Should().BeEmpty();
	}

	[Fact]
	public async Task EmptyBatch_ReturnsEmptyDictionary()
	{
		var result = await Mediator.Send(
			new GetTransactionsByIdsQuery(Array.Empty<Guid>()),
			CancellationToken);

		result.Transactions.Should().BeEmpty();
	}

	[Fact]
	public async Task ReversedTransaction_IsReturnedById()
	{
		var id = Scenario.Reversed.Id;

		var result = await Mediator.Send(new GetTransactionsByIdsQuery(id), CancellationToken);

		result.Transactions.Keys.Should().Equal(id);
	}

	public sealed class TransactionsByIdsScenario : ReadScenario
	{
		public TransactionsByIdsScenario() => RegisterBasicContext<BalanceTestContext>();

		public Transaction First { get; private set; } = null!;

		public Transaction Second { get; private set; } = null!;

		public Transaction Reversed { get; private set; } = null!;

		protected override IEnumerable<object> Seed()
		{
			var context = Get<BalanceTestContext>();
			Reversed = context.Transactions[0];
			Reversed.Reverse(context.Users[0].Id);
			Reversed.Apply(
				Context.Set<OrganizationBalance>().Single(balance =>
					balance.OrganizationId == Reversed.SenderId && balance.CurrencyId == Reversed.CurrencyId),
				Context.Set<OrganizationBalance>().Single(balance =>
					balance.OrganizationId == Reversed.ReceiverId && balance.CurrencyId == Reversed.CurrencyId));
			First = CreateTransaction(context);
			Second = CreateTransaction(context);
			return [First, Second];
		}

		private Transaction CreateTransaction(BalanceTestContext context)
			=> new TransactionBuilder(Faker)
				.WithSenderId(context.Users[0].Id)
				.WithReceiverId(context.Users[1].Id)
				.WithCurrencyId(context.Currencies[0].Id)
				.WithAmount(50m)
				.Build();
	}
}
