using Abstractions.Models;
using Enums;
using FluentAssertions;
using Main.Application.Handlers.Balance.GetTransactions;
using Main.Entities.Balance;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
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
        result.Transactions[0].Sender.User?.Id.Should().Be(senderId);
    }

    [Fact]
    public async Task GetTransactions_ByReceiver_ReturnsTransactions()
    {
        var receiverId = TestContext.Users[1].Id;

        var result = await Mediator.Send(GetQuery(receiverId: receiverId));

        result.Transactions.Should().ContainSingle();
        result.Transactions[0].Receiver.User?.Id.Should().Be(receiverId);
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
    public async Task GetTransactions_WithOr_ReturnsTransactionsWhereUserIsSenderOrReceiver()
    {
        var userId = TestContext.Users[0].Id;

        var result = await Mediator.Send(GetQuery(
            senderId: userId,
            receiverId: userId,
            logicalOperation: LogicalOperation.Or));

        result.Transactions.Should().NotBeEmpty();
        result.Transactions.Should().OnlyContain(x =>
            x.Sender.User != null && x.Sender.User.Id == userId ||
            x.Receiver.User != null && x.Receiver.User.Id == userId);
    }

    [Fact]
    public async Task GetTransactions_SkipReversedFalse_ReturnsReversedTransactions()
    {
        var transaction = await ReverseSeedTransaction();

        var result = await Mediator.Send(GetQuery(
            senderId: transaction.SenderId,
            skipReversed: false));

        result.Transactions.Should().Contain(x => x.Id == transaction.Id);
    }

    [Fact]
    public async Task GetTransactions_SkipReversedTrue_DoesNotReturnReversedTransactions()
    {
        var transaction = await ReverseSeedTransaction();

        var result = await Mediator.Send(GetQuery(
            senderId: transaction.SenderId,
            skipReversed: true));

        result.Transactions.Should().NotContain(x => x.Id == transaction.Id);
    }

    [Fact]
    public async Task GetTransactions_SkipReversedTrue_ReturnsCompletionProfileAppliedTransactions()
    {
        var transaction = await CreateCompletionProfileAppliedTransaction();

        var result = await Mediator.Send(GetQuery(
            senderId: transaction.SenderId,
            skipReversed: true));

        result.Transactions.Should().Contain(x => x.Id == transaction.Id);
    }

    [Fact]
    public async Task GetTransactions_SameSenderAndReceiver_ThrowsValidationException()
    {
        var userId = TestContext.Users[0].Id;

        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(GetQuery(userId, userId)));
    }

    [Fact]
    public async Task GetTransactions_WithoutSenderAndReceiver_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(GetQuery()));
    }

    [Fact]
    public async Task GetTransactions_InvalidRange_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(GetQuery(
                rangeStart: DateTime.UtcNow,
                rangeEnd: DateTime.UtcNow.AddDays(-1))));
    }

    private GetTransactionsQuery GetQuery(
        Guid? senderId = null,
        Guid? receiverId = null,
        int? currencyId = null,
        int size = 20,
        DateTime? rangeStart = null,
        DateTime? rangeEnd = null,
        LogicalOperation logicalOperation = LogicalOperation.And,
        bool skipReversed = false)
    {
        return new GetTransactionsQuery(
            rangeStart ?? DateTime.UtcNow.AddDays(-10),
            rangeEnd ?? DateTime.UtcNow.AddDays(1),
            currencyId,
            senderId,
            receiverId,
            logicalOperation,
            new Cursor<(Guid id, DateTime dt)>((Guid.Empty, DateTime.MinValue), size),
            skipReversed);
    }

    private async Task<Main.Entities.Balance.Transaction> ReverseSeedTransaction()
    {
        var transaction = await Context.Transactions
            .FirstAsync(x => x.Id == TestContext.Transactions[0].Id);

        var senderBalance = await Context.UserBalances
            .FirstAsync(x => x.UserId == transaction.SenderId && x.CurrencyId == transaction.CurrencyId);
        var receiverBalance = await Context.UserBalances
            .FirstAsync(x => x.UserId == transaction.ReceiverId && x.CurrencyId == transaction.CurrencyId);

        transaction.Reverse(TestContext.Users[0].Id);
        transaction.Apply(senderBalance, receiverBalance);
        await Context.SaveChangesAsync();

        return transaction;
    }

    private async Task<Main.Entities.Balance.Transaction> CreateCompletionProfileAppliedTransaction()
    {
        var sender = TestContext.Users[0];
        var receiver = TestContext.Users[1];
        var currency = TestContext.Currencies[0];
        var senderBalance = await Context.UserBalances
            .FirstAsync(x => x.UserId == sender.Id && x.CurrencyId == currency.Id);
        var receiverBalance = await Context.UserBalances
            .FirstAsync(x => x.UserId == receiver.Id && x.CurrencyId == currency.Id);
        var senderProfile = await Context.Set<UserFinancialProfile>()
            .FirstAsync(x => x.UserId == sender.Id);
        var receiverProfile = await Context.Set<UserFinancialProfile>()
            .FirstAsync(x => x.UserId == receiver.Id);
        senderProfile.Credit(100m);

        var transaction = Main.Entities.Balance.Transaction.Create(
            sender.Id,
            receiver.Id,
            currency.Id,
            Main.Enums.Balances.TransactionType.Transfer,
            100m,
            DateTime.UtcNow.AddDays(-1),
            Main.Enums.Balances.TransactionSourceType.Manual);

        transaction.Complete();
        transaction.Apply(senderBalance, receiverBalance);
        new TransactionFinancialProfileService().Apply(
            transaction,
            senderProfile,
            receiverProfile,
            100m,
            Guid.NewGuid());

        await Context.AddAsync(transaction);
        await Context.SaveChangesAsync();

        return transaction;
    }
}
