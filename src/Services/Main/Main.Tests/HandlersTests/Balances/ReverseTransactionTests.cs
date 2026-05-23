using FluentAssertions;
using Main.Application.Handlers.Balance.ReverseTransaction;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts.Balance;
using ValidationException = FluentValidation.ValidationException;

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
        var reversedBy = TestContext.Users[2].Id;

        await Mediator.Send(new ReverseTransactionCommand(transaction.Id, reversedBy));

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
    public async Task ReverseTransaction_UnknownTransaction_ThrowsTransactionNotFoundException()
    {
        var reversedBy = TestContext.Users[0].Id;

        await Assert.ThrowsAsync<TransactionNotFoundException>(() =>
            Mediator.Send(new ReverseTransactionCommand(Guid.NewGuid(), reversedBy)));
    }

    [Fact]
    public async Task ReverseTransaction_UnknownUser_ThrowsDbValidationException()
    {
        var transaction = TestContext.Transactions[0];

        await Assert.ThrowsAsync<DbValidationException>(() =>
            Mediator.Send(new ReverseTransactionCommand(transaction.Id, Guid.NewGuid())));
    }

    [Fact]
    public async Task ReverseTransaction_EmptyTransactionId_ThrowsValidationException()
    {
        var reversedBy = TestContext.Users[0].Id;

        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(new ReverseTransactionCommand(Guid.Empty, reversedBy)));
    }

    [Fact]
    public async Task ReverseTransaction_EmptyUserId_ThrowsValidationException()
    {
        var transaction = TestContext.Transactions[0];

        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(new ReverseTransactionCommand(transaction.Id, Guid.Empty)));
    }
}