using Abstractions.Models;
using Enums;
using FluentAssertions;
using Main.Application.Handlers.Balance.GetTransactions;
using Main.Entities.Balance;
using Main.Entities.Organization;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Balance;

namespace Tests.HandlersTests.Balances;

public class GetTransactionsTests : IntegrationTest
{
	public GetTransactionsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<BalanceTestContext>();
	}

	private BalanceTestContext TestContext => GetContext<BalanceTestContext>();

	[Fact]
	public async Task GetTransactions_BySender_ReturnsTransactions()
	{
		var senderId = TestContext.Users[0].Id;

		var result = await Mediator.Send(GetQuery(senderId));

		result.Transactions.Should().ContainSingle();
		result.Transactions[0].Sender.Id.Should().Be(senderId);
	}

	[Fact]
	public async Task GetTransactions_ByReceiver_ReturnsTransactions()
	{
		var receiverId = TestContext.Users[1].Id;

		var result = await Mediator.Send(GetQuery(receiverId: receiverId));

		result.Transactions.Should().ContainSingle();
		result.Transactions[0].Receiver.Id.Should().Be(receiverId);
	}

	[Fact]
	public async Task GetTransactions_ByCurrency_ReturnsOnlyCurrencyTransactions()
	{
		var currencyId = TestContext.Currencies[0].Id;
		var senderId = TestContext.Users[0].Id;

		var result = await Mediator.Send(GetQuery(senderId, currencyId: currencyId));

		result.Transactions.Should().OnlyContain(x => x.CurrencyId == currencyId);
	}

	[Fact]
	public async Task GetTransactions_WithSizeLimit_ReturnsLimitedPage()
	{
		var receiverId = TestContext.Users[1].Id;

		var result = await Mediator.Send(GetQuery(receiverId: receiverId, size: 1));

		result.Transactions.Should().HaveCount(1);
	}

	[Fact]
	public async Task GetTransactions_WithCursor_ReturnsTransactionsByDateDescendingWithoutDuplicates()
	{
		var baseDate = DateTime.UtcNow.Date.AddHours(12);
		var transactions = await AddTransactions(
			baseDate.AddHours(-2),
			baseDate.AddHours(-1),
			baseDate);
		var senderId = transactions[0].SenderId;
		var rangeStart = baseDate.AddHours(-3);
		var rangeEnd = baseDate.AddHours(1);

		var firstPage = await Mediator.Send(
			GetQuery(
				senderId,
				size: 2,
				rangeStart: rangeStart,
				rangeEnd: rangeEnd));

		firstPage.Transactions.Select(x => x.Id).Should().Equal(transactions[2].Id, transactions[1].Id);

		var cursor = firstPage.Transactions[^1];
		var secondPage = await Mediator.Send(
			GetQuery(
				senderId,
				size: 2,
				rangeStart: rangeStart,
				rangeEnd: rangeEnd,
				cursorId: cursor.Id,
				cursorDate: cursor.TransactionDate));

		secondPage.Transactions.Should().ContainSingle().Which.Id.Should().Be(transactions[0].Id);
		secondPage
			.Transactions
			.Select(x => x.Id)
			.Should()
			.NotIntersectWith(firstPage.Transactions.Select(x => x.Id));
	}

	[Fact]
	public async Task GetTransactions_AtRangeEnd_IncludesBoundaryAndExcludesLaterTransaction()
	{
		var boundary = DateTime.UtcNow.Date;
		var transactions = await AddTransactions(
			boundary.AddMilliseconds(-1),
			boundary,
			boundary.AddMilliseconds(1));

		var result = await Mediator.Send(
			GetQuery(
				transactions[0].SenderId,
				rangeStart: boundary.AddHours(-1),
				rangeEnd: boundary));

		result.Transactions.Select(x => x.Id).Should().Equal(transactions[1].Id, transactions[0].Id);
		result.Transactions.Should().NotContain(x => x.Id == transactions[2].Id);
	}

	[Fact]
	public async Task GetTransactions_WithOr_ReturnsTransactionsWhereUserIsSenderOrReceiver()
	{
		var userId = TestContext.Users[0].Id;

		var result = await Mediator.Send(
			GetQuery(
				userId,
				userId,
				logicalOperation: LogicalOperation.Or));

		result.Transactions.Should().NotBeEmpty();
		result.Transactions.Should().OnlyContain(x => x.Sender.Id == userId || x.Receiver.Id == userId);
	}

	[Fact]
	public async Task GetTransactions_SkipReversedFalse_ReturnsReversedTransactions()
	{
		var transaction = await ReverseSeedTransaction();

		var result = await Mediator.Send(GetQuery(transaction.SenderId, skipReversed: false));

		result.Transactions.Should().Contain(x => x.Id == transaction.Id);
	}

	[Fact]
	public async Task GetTransactions_SkipReversedTrue_DoesNotReturnReversedTransactions()
	{
		var transaction = await ReverseSeedTransaction();

		var result = await Mediator.Send(GetQuery(transaction.SenderId, skipReversed: true));

		result.Transactions.Should().NotContain(x => x.Id == transaction.Id);
	}

	[Fact]
	public async Task GetTransactions_SkipReversedTrue_ReturnsCompletionProfileAppliedTransactions()
	{
		var transaction = await CreateCompletionProfileAppliedTransaction();

		var result = await Mediator.Send(GetQuery(transaction.SenderId, skipReversed: true));

		result.Transactions.Should().Contain(x => x.Id == transaction.Id);
	}

	[Fact]
	public async Task GetTransactions_SameSenderAndReceiver_ThrowsValidationException()
	{
		var userId = TestContext.Users[0].Id;

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(GetQuery(userId, userId)));
	}

	[Fact]
	public async Task GetTransactions_WithoutSenderAndReceiver_ThrowsValidationException()
	{
		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(GetQuery()));
	}

	[Fact]
	public async Task GetTransactions_InvalidRange_ThrowsValidationException()
	{
		await Assert.ThrowsAsync<ValidationException>(() =>
			Mediator.Send(GetQuery(rangeStart: DateTime.UtcNow, rangeEnd: DateTime.UtcNow.AddDays(-1))));
	}

	private GetTransactionsQuery GetQuery(
		Guid? senderId = null,
		Guid? receiverId = null,
		int? currencyId = null,
		int size = 20,
		DateTime? rangeStart = null,
		DateTime? rangeEnd = null,
		LogicalOperation logicalOperation = LogicalOperation.And,
		bool skipReversed = false,
		Guid? cursorId = null,
		DateTime? cursorDate = null)
	{
		return new GetTransactionsQuery(
			new RangeModel<DateTime>(rangeStart, rangeEnd),
			currencyId,
			senderId,
			receiverId,
			logicalOperation,
			new Cursor<(Guid id, DateTime dt)>(
				(cursorId ?? Guid.Empty, cursorDate ?? DateTime.MinValue),
				size),
			skipReversed);
	}

	private async Task<IReadOnlyList<Transaction>> AddTransactions(params DateTime[] dates)
	{
		var sender = TestContext.Users[0];
		var receiver = TestContext.Users[1];
		var currency = TestContext.Currencies[0];
		var transactions = dates
			.Select(date => Transaction.Create(
				sender.Id,
				receiver.Id,
				currency.Id,
				TransactionType.Transfer,
				100m,
				date,
				TransactionSourceType.Manual))
			.ToList();

		await Context.AddRangeAsync(transactions);
		await Context.SaveChangesAsync();
		return transactions;
	}

	private async Task<Transaction> ReverseSeedTransaction()
	{
		var transaction = await Context.Transactions.FirstAsync(x => x.Id == TestContext.Transactions[0].Id);

		var senderBalance = await Context.UserBalances.FirstAsync(x =>
			x.OrganizationId == transaction.SenderId && x.CurrencyId == transaction.CurrencyId);
		var receiverBalance = await Context.UserBalances.FirstAsync(x =>
			x.OrganizationId == transaction.ReceiverId && x.CurrencyId == transaction.CurrencyId);

		transaction.Reverse(TestContext.Users[0].Id);
		transaction.Apply(senderBalance, receiverBalance);
		await Context.SaveChangesAsync();

		return transaction;
	}

	private async Task<Transaction> CreateCompletionProfileAppliedTransaction()
	{
		var sender = TestContext.Users[0];
		var receiver = TestContext.Users[1];
		var currency = TestContext.Currencies[0];
		var senderBalance = await Context.UserBalances.FirstAsync(x =>
			x.OrganizationId == sender.Id && x.CurrencyId == currency.Id);
		var receiverBalance = await Context.UserBalances.FirstAsync(x =>
			x.OrganizationId == receiver.Id && x.CurrencyId == currency.Id);
		var senderProfile = await Context
			.Set<OrganizationFinancialProfile>()
			.FirstAsync(x => x.OrganizationId == sender.Id);
		var receiverProfile = await Context
			.Set<OrganizationFinancialProfile>()
			.FirstAsync(x => x.OrganizationId == receiver.Id);

		var transaction = Transaction.Create(
			sender.Id,
			receiver.Id,
			currency.Id,
			TransactionType.Transfer,
			100m,
			DateTime.UtcNow.AddDays(-1),
			TransactionSourceType.Manual);

		transaction.Complete();
		transaction.Apply(senderBalance, receiverBalance);
		new TransactionFinancialProfileService().Apply(
			transaction,
			senderProfile,
			receiverProfile,
			0m,
			100m,
			100m);

		await Context.AddAsync(transaction);
		await Context.SaveChangesAsync();

		return transaction;
	}
}
