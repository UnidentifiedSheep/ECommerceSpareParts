using FluentAssertions;
using Main.Application.Handlers.Balance.ReverseTransaction;
using Main.Entities.Exceptions;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders.Balance;
using Tests.TestContexts;
using Tests.TestContexts.Balance;

namespace Tests.HandlersTests.Balances;

public class ReverseTransactionTests : IntegrationTest
{
    public ReverseTransactionTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<BalanceTestContext>();
    }

    private BalanceTestContext TestContext => GetContext<BalanceTestContext>();

    [Fact]
    public async Task ReverseTransaction_ValidData_Succeeds()
    {
        var transaction = TestContext.Transactions[0];
        var reversedBy = GetContext<UserContextTestContext>().UserContext.UserId;

        await Mediator.Send(new ReverseTransactionCommand(transaction.Id));

        var reversed = await Context.Transactions
            .AsNoTracking()
            .FirstAsync(x => x.Id == transaction.Id);

        reversed.IsReversed.Should().BeTrue();
        reversed.IsReversalApplied.Should().BeTrue();
        reversed.ReversedBy.Should().Be(reversedBy);
        reversed.ReversedAt.Should().NotBeNull();

        var senderBalance = await Context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == transaction.SenderId && x.CurrencyId == transaction.CurrencyId);
        var receiverBalance = await Context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == transaction.ReceiverId && x.CurrencyId == transaction.CurrencyId);

        senderBalance.Balance.Should().Be(0m);
        receiverBalance.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task ReverseTransaction_UserModeAndNotManualSource_ThrowsTransactionSourceCannotBeReversedByUserException()
    {
        var transaction = await CreateAppliedTransaction(TransactionSourceType.Purchase);

        await Assert.ThrowsAsync<TransactionSourceCannotBeReversedByUserException>(() =>
            Mediator.Send(new ReverseTransactionCommand(transaction.Id)));
    }

    [Fact]
    public async Task ReverseTransaction_SystemModeAndNotManualSource_Succeeds()
    {
        var transaction = await CreateAppliedTransaction(TransactionSourceType.Purchase);

        await Mediator.Send(new ReverseTransactionCommand(
            transaction.Id,
            TransactionReversalMode.System, 
true));

        var reversed = await Context.Transactions
            .AsNoTracking()
            .FirstAsync(x => x.Id == transaction.Id);

        reversed.IsReversed.Should().BeTrue();
        reversed.IsReversalApplied.Should().BeTrue();
    }

    [Fact]
    public async Task ReverseTransaction_UnknownTransaction_ThrowsTransactionNotFoundException()
    {
        var reversedBy = TestContext.Users[0].Id;

        await Assert.ThrowsAsync<TransactionNotFoundException>(() =>
            Mediator.Send(new ReverseTransactionCommand(Guid.NewGuid())));
    }

    [Fact]
    public async Task ReverseTransaction_EmptyTransactionId_ThrowsValidationException()
    {
        var reversedBy = TestContext.Users[0].Id;

        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(new ReverseTransactionCommand(Guid.Empty)));
    }

    private async Task<Main.Entities.Balance.Transaction> CreateAppliedTransaction(
        TransactionSourceType sourceType)
    {
        var currencyId = TestContext.Currencies[0].Id;
        var senderId = TestContext.Users[0].Id;
        var receiverId = TestContext.Users[1].Id;

        var senderBalance = await Context.UserBalances
            .FirstAsync(x => x.UserId == senderId && x.CurrencyId == currencyId);
        var receiverBalance = await Context.UserBalances
            .FirstAsync(x => x.UserId == receiverId && x.CurrencyId == currencyId);

        var transaction = new TransactionBuilder(Faker)
            .WithSenderId(senderId)
            .WithReceiverId(receiverId)
            .WithCurrencyId(currencyId)
            .WithAmount(10m)
            .WithSourceType(sourceType)
            .WithBalances(senderBalance, receiverBalance)
            .Completed()
            .Applied()
            .Build();

        await Context.Transactions.AddAsync(transaction);
        await Context.SaveChangesAsync();

        return transaction;
    }
}
