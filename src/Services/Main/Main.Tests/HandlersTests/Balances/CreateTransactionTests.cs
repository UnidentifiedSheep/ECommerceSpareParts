using FluentAssertions;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Static;
using Main.Entities.Balance;
using Main.Entities.Exceptions;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;

namespace Tests.HandlersTests.Balances;

public class CreateTransactionTests : IntegrationTest
{
    public CreateTransactionTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<UsersTestContext>();
        RegisterBasicContext<UserContextTestContext>();
        RegisterBasicContext<CurrencyTestContext>();
    }

    private UsersTestContext UsersContext => GetContext<UsersTestContext>();
    private CurrencyTestContext CurrencyContext => GetContext<CurrencyTestContext>();

    [Fact]
    public async Task CreateTransaction_ValidData_Succeeds()
    {
        var sender = UsersContext.Users.ElementAt(0);
        var receiver = UsersContext.Users.ElementAt(1);
        var currency = CurrencyContext.Currencies[0];
        var amount = 125.50m;
        var transactionDateTime = DateTime.UtcNow;

        await CreditProfile(sender.Id, amount);

        var result = await Mediator.Send(
            new CreateTransactionCommand(
                sender.Id,
                receiver.Id,
                amount,
                currency.Id,
                transactionDateTime,
                TransactionSourceType.Manual));

        var transaction = await Context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == result.Transaction.Id);

        transaction.Should().NotBeNull();
        transaction.SenderId.Should().Be(sender.Id);
        transaction.ReceiverId.Should().Be(receiver.Id);
        transaction.CurrencyId.Should().Be(currency.Id);
        transaction.Amount.Should().Be(amount);
        transaction.IsCompleted.Should().BeTrue();
        transaction.IsCompletionApplied.Should().BeTrue();

        var senderBalance = await Context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == sender.Id && x.CurrencyId == currency.Id);
        var receiverBalance = await Context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == receiver.Id && x.CurrencyId == currency.Id);

        senderBalance.Balance.Should().Be(amount);
        receiverBalance.Balance.Should().Be(-amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateTransaction_InvalidAmount_ThrowsValidationException(decimal amount)
    {
        var command = GetValidCommand();

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command with { Amount = amount }));
    }

    [Fact]
    public async Task CreateTransaction_TooOldDate_ThrowsValidationException()
    {
        var command = GetValidCommand();

        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(command with { TransactionDateTime = DateTime.UtcNow.AddMonths(-3) }));
    }

    [Fact]
    public async Task CreateTransaction_FutureDate_ThrowsValidationException()
    {
        var command = GetValidCommand();

        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(command with { TransactionDateTime = DateTime.UtcNow.AddHours(2) }));
    }

    [Fact]
    public async Task CreateTransaction_MissingUser_ThrowsDbValidationException()
    {
        var command = GetValidCommand();

        var exception = await Assert.ThrowsAsync<DbValidationException>(() =>
            Mediator.Send(command with { SenderId = Guid.NewGuid() }));

        exception.Failures[0].ErrorName.Should().Be(ApplicationErrors.UsersNotFound);
    }

    [Fact]
    public async Task CreateTransaction_MissingCurrency_ThrowsDbValidationException()
    {
        var command = GetValidCommand();

        var exception = await Assert.ThrowsAsync<DbValidationException>(() =>
            Mediator.Send(command with { CurrencyId = int.MaxValue }));

        exception.Failures[0].ErrorName.Should().Be(ApplicationErrors.CurrencyNotFound);
    }

    [Fact]
    public async Task
        CreateTransaction_UserModeAndSystemSender_ThrowsTransactionWithSystemUserCannotBeCreatedByUserException()
    {
        var command = GetValidCommand() with
        {
            SenderId = GetContext<UserContextTestContext>().SystemUser.Id
        };

        await Assert.ThrowsAsync<TransactionWithSystemUserCannotBeCreatedByUserException>(() =>
            Mediator.Send(command));
    }

    [Fact]
    public async Task
        CreateTransaction_UserModeAndSystemReceiver_ThrowsTransactionWithSystemUserCannotBeCreatedByUserException()
    {
        var command = GetValidCommand() with
        {
            ReceiverId = GetContext<UserContextTestContext>().SystemUser.Id
        };

        await Assert.ThrowsAsync<TransactionWithSystemUserCannotBeCreatedByUserException>(() =>
            Mediator.Send(command));
    }

    [Fact]
    public async Task CreateTransaction_SystemModeAndSystemReceiver_Succeeds()
    {
        var command = GetValidCommand() with
        {
            ReceiverId = GetContext<UserContextTestContext>().SystemUser.Id,
            Mode = TransactionCreationMode.System
        };

        await CreditProfile(command.SenderId, command.Amount);

        var result = await Mediator.Send(command);

        var transaction = await Context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == result.Transaction.Id);

        transaction.Should().NotBeNull();
        transaction!.ReceiverId.Should().Be(command.ReceiverId);
    }

    private CreateTransactionCommand GetValidCommand()
    {
        var sender = UsersContext.Users.ElementAt(0);
        var receiver = UsersContext.Users.ElementAt(1);
        var currency = CurrencyContext.Currencies[0];

        return new CreateTransactionCommand(
            sender.Id,
            receiver.Id,
            100m,
            currency.Id,
            DateTime.UtcNow,
            TransactionSourceType.Manual);
    }

    private async Task CreditProfile(Guid userId, decimal amount)
    {
        var profile = UserFinancialProfile.Create(userId);
        profile.Credit(amount);

        await Context.AddAsync(profile);
        await Context.SaveChangesAsync();
    }
}