using FluentAssertions;
using Main.Application.Handlers.Balance;
using Main.Entities.Balance;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders.Balance;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Balance;

namespace Tests.HandlersTests.Balances;

public sealed class GetTransactionsByIdsTests : IntegrationTest
{
	public GetTransactionsByIdsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<BalanceTestContext>();
	}

	private BalanceTestContext TestContext => GetContext<BalanceTestContext>();

	[Fact]
	public async Task Batch_ReturnsOnlyRequestedTransactionsWithoutDuplicates()
	{
		var first = await CreateTransaction();
		var second = await CreateTransaction();

		var result = await Mediator.Send(
			new GetTransactionsByIdsQuery([first.Id, second.Id, first.Id, Guid.NewGuid()]), CancellationToken);

		result.Transactions.Keys.Should().BeEquivalentTo([first.Id, second.Id]);
	}

	[Fact]
	public async Task SingleTransaction_IsReturnedById()
	{
		var id = TestContext.Transactions[0].Id;

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
		var result = await Mediator.Send(new GetTransactionsByIdsQuery(Array.Empty<Guid>()), CancellationToken);

		result.Transactions.Should().BeEmpty();
	}

	[Fact]
	public async Task ReversedTransaction_IsReturnedById()
	{
		var transaction = TestContext.Transactions[0];
		transaction.Reverse(TestContext.Users[0].Id);
		var senderBalance = await Context.UserBalances
			.SingleAsync(balance =>
				balance.OrganizationId == transaction.SenderId &&
				balance.CurrencyId == transaction.CurrencyId,
				CancellationToken);
		var receiverBalance = await Context.UserBalances
			.SingleAsync(balance =>
				balance.OrganizationId == transaction.ReceiverId &&
				balance.CurrencyId == transaction.CurrencyId,
				CancellationToken);
		transaction.Apply(senderBalance, receiverBalance);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Mediator.Send(new GetTransactionsByIdsQuery(transaction.Id), CancellationToken);

		result.Transactions.Keys.Should().Equal(transaction.Id);
	}

	private Task<Transaction> CreateTransaction() =>
		new TransactionBuilder(Faker)
			.WithSenderId(TestContext.Users[0].Id)
			.WithReceiverId(TestContext.Users[1].Id)
			.WithCurrencyId(TestContext.Currencies[0].Id)
			.WithAmount(50m)
			.BuildAndAddToDb(Context);
}
