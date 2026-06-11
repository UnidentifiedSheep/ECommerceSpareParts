using FluentAssertions;
using Main.Application.Handlers.Balance;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;

namespace Tests.HandlersTests.Balances;

public class CreateSystemTransactionTests : IntegrationTest
{
    public CreateSystemTransactionTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<UsersTestContext>();
        RegisterBasicContext<UserContextTestContext>();
        RegisterBasicContext<CurrencyTestContext>();
    }

    private UsersTestContext UsersContext => GetContext<UsersTestContext>();
    private UserContextTestContext UserContext => GetContext<UserContextTestContext>();
    private CurrencyTestContext CurrencyContext => GetContext<CurrencyTestContext>();

    [Fact]
    public async Task CreateSystemTransaction_UserToSystem_CreatesTransaction()
    {
        var user = UsersContext.Users.ElementAt(0);
        var systemUser = UserContext.SystemUser;
        var currency = CurrencyContext.Currencies[0];
        var amount = 125.50m;

        var result = await Mediator.Send(new CreateSystemTransactionCommand(
            user.Id,
            amount,
            currency.Id,
            DateTime.UtcNow,
            SystemTransactionDirection.UserToSystem));

        var transaction = await Context.Transactions
            .AsNoTracking()
            .FirstAsync(x => x.Id == result.Transaction.Id);

        transaction.SenderId.Should().Be(user.Id);
        transaction.ReceiverId.Should().Be(systemUser.Id);
        transaction.Amount.Should().Be(amount);
        transaction.SourceType.Should().Be(TransactionSourceType.Manual);
        transaction.IsCompleted.Should().BeTrue();

        var userBalance = await Context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == user.Id && x.CurrencyId == currency.Id);
        var systemBalance = await Context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == systemUser.Id && x.CurrencyId == currency.Id);

        userBalance.Balance.Should().Be(-amount);
        systemBalance.Balance.Should().Be(amount);
    }

    [Fact]
    public async Task CreateSystemTransaction_SystemToUser_CreatesTransaction()
    {
        var user = UsersContext.Users.ElementAt(0);
        var systemUser = UserContext.SystemUser;
        var currency = CurrencyContext.Currencies[0];
        var amount = 75m;

        var result = await Mediator.Send(new CreateSystemTransactionCommand(
            user.Id,
            amount,
            currency.Id,
            DateTime.UtcNow,
            SystemTransactionDirection.SystemToUser));

        var transaction = await Context.Transactions
            .AsNoTracking()
            .FirstAsync(x => x.Id == result.Transaction.Id);

        transaction.SenderId.Should().Be(systemUser.Id);
        transaction.ReceiverId.Should().Be(user.Id);
        transaction.Amount.Should().Be(amount);
        transaction.SourceType.Should().Be(TransactionSourceType.Manual);
        transaction.IsCompleted.Should().BeTrue();

        var userBalance = await Context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == user.Id && x.CurrencyId == currency.Id);
        var systemBalance = await Context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == systemUser.Id && x.CurrencyId == currency.Id);

        userBalance.Balance.Should().Be(amount);
        systemBalance.Balance.Should().Be(-amount);
    }
}
