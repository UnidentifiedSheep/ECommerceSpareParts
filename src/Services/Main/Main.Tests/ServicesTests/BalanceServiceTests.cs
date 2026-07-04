using FluentAssertions;
using Main.Application.Interfaces.Services;
using Main.Entities.Balance;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders.Balance;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;

namespace Tests.ServicesTests;

public class BalanceServiceTests : IntegrationTest
{
    private IBalanceService _service = null!;

    public BalanceServiceTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<UsersTestContext>();
        RegisterBasicContext<CurrencyTestContext>();
    }

    private UsersTestContext UsersContext => GetContext<UsersTestContext>();
    private UserContextTestContext UserContext => GetContext<UserContextTestContext>();
    private CurrencyTestContext CurrencyContext => GetContext<CurrencyTestContext>();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _service = Scope.ServiceProvider.GetRequiredService<IBalanceService>();
    }

    [Fact]
    public async Task ChangeSenderReceiverBalancesAsync_ManualUserToUser_SpendsAvailableAndDepositsWallet()
    {
        var sender = UsersContext.Users.ElementAt(0);
        var receiver = UsersContext.Users.ElementAt(1);
        var currency = CurrencyContext.Currencies[0];
        await Context.AddAsync(
            new UserFinancialProfileBuilder(Faker)
                .WithUserId(sender.Id)
                .WithBalance(700m)
                .Build());
        await Context.SaveChangesAsync();

        var transaction = new TransactionBuilder(Faker)
            .WithSenderId(sender.Id)
            .WithReceiverId(receiver.Id)
            .WithCurrencyId(currency.Id)
            .WithAmount(700m)
            .WithSourceType(TransactionSourceType.Manual)
            .Completed()
            .Build();

        await _service.ChangeSenderReceiverBalancesAsync(transaction);
        await Context.SaveChangesAsync();

        var senderProfile = await GetProfile(sender.Id);
        var receiverProfile = await GetProfile(receiver.Id);
        senderProfile.Balance.Should().Be(0m);
        receiverProfile.Balance.Should().Be(700m);

        (await GetUserBalance(sender.Id, currency.Id)).Balance.Should().Be(700m);
        (await GetUserBalance(receiver.Id, currency.Id)).Balance.Should().Be(-700m);
    }

    [Fact]
    public async Task ChangeSenderReceiverBalancesAsync_ManualUserToSystem_CreditsUserBalance()
    {
        var user = UsersContext.Users.ElementAt(0);
        var systemUser = UserContext.SystemUser;
        var currency = CurrencyContext.Currencies[0];
        await Context.AddAsync(
            new UserFinancialProfileBuilder(Faker)
                .WithUserId(user.Id)
                .WithBalance(300m)
                .Build());
        await Context.SaveChangesAsync();

        var transaction = new TransactionBuilder(Faker)
            .WithSenderId(user.Id)
            .WithReceiverId(systemUser.Id)
            .WithCurrencyId(currency.Id)
            .WithAmount(250m)
            .WithSourceType(TransactionSourceType.Manual)
            .Completed()
            .Build();

        await _service.ChangeSenderReceiverBalancesAsync(transaction);
        await Context.SaveChangesAsync();

        var userProfile = await GetProfile(user.Id);
        userProfile.Balance.Should().Be(550m);

        (await GetUserBalance(user.Id, currency.Id)).Balance.Should().Be(250m);
        (await GetUserBalance(systemUser.Id, currency.Id)).Balance.Should().Be(-250m);
    }

    [Fact]
    public async Task ChangeSenderReceiverBalancesAsync_ManualSystemToUser_DebitsUserBalance()
    {
        var user = UsersContext.Users.ElementAt(0);
        var systemUser = UserContext.SystemUser;
        var currency = CurrencyContext.Currencies[0];
        await Context.AddAsync(
            new UserFinancialProfileBuilder(Faker)
                .WithUserId(user.Id)
                .WithBalance(250m)
                .Build());
        await Context.SaveChangesAsync();

        var transaction = new TransactionBuilder(Faker)
            .WithSenderId(systemUser.Id)
            .WithReceiverId(user.Id)
            .WithCurrencyId(currency.Id)
            .WithAmount(250m)
            .WithSourceType(TransactionSourceType.Manual)
            .Completed()
            .Build();

        await _service.ChangeSenderReceiverBalancesAsync(transaction);
        await Context.SaveChangesAsync();

        var userProfile = await GetProfile(user.Id);
        userProfile.Balance.Should().Be(0m);

        (await GetUserBalance(systemUser.Id, currency.Id)).Balance.Should().Be(250m);
        (await GetUserBalance(user.Id, currency.Id)).Balance.Should().Be(-250m);
    }

    [Theory]
    [InlineData(TransactionSourceType.Purchase)]
    [InlineData(TransactionSourceType.Logistic)]
    public async Task ChangeSenderReceiverBalancesAsync_SystemSettlementUserToSystem_CreditsUserBalance(
        TransactionSourceType sourceType)
    {
        var user = UsersContext.Users.ElementAt(0);
        var systemUser = UserContext.SystemUser;
        var currency = CurrencyContext.Currencies[0];
        await Context.AddAsync(
            new UserFinancialProfileBuilder(Faker)
                .WithUserId(user.Id)
                .WithBalance(100m)
                .Build());
        await Context.SaveChangesAsync();

        var transaction = new TransactionBuilder(Faker)
            .WithSenderId(user.Id)
            .WithReceiverId(systemUser.Id)
            .WithCurrencyId(currency.Id)
            .WithAmount(100m)
            .WithSourceType(sourceType)
            .Completed()
            .Build();

        await _service.ChangeSenderReceiverBalancesAsync(transaction);
        await Context.SaveChangesAsync();

        var userProfile = await GetProfile(user.Id);
        userProfile.Balance.Should().Be(200m);
    }

    [Fact]
    public async Task ChangeSenderReceiverBalancesAsync_SystemSettlementSystemToUser_DebitsUserBalance()
    {
        var buyer = UsersContext.Users.ElementAt(0);
        var systemUser = UserContext.SystemUser;
        var currency = CurrencyContext.Currencies[0];
        await Context.AddAsync(
            new UserFinancialProfileBuilder(Faker)
                .WithUserId(buyer.Id)
                .WithBalance(100m)
                .Build());
        await Context.SaveChangesAsync();

        var transaction = new TransactionBuilder(Faker)
            .WithSenderId(systemUser.Id)
            .WithReceiverId(buyer.Id)
            .WithCurrencyId(currency.Id)
            .WithAmount(100m)
            .WithSourceType(TransactionSourceType.Sale)
            .Completed()
            .Build();

        await _service.ChangeSenderReceiverBalancesAsync(transaction);
        await Context.SaveChangesAsync();

        var buyerProfile = await GetProfile(buyer.Id);
        buyerProfile.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task ChangeSenderReceiverBalancesAsync_ReverseSystemSettlementUserToSystem_RollsBackBalance()
    {
        var supplier = UsersContext.Users.ElementAt(0);
        var systemUser = UserContext.SystemUser;
        var currency = CurrencyContext.Currencies[0];
        await Context.AddAsync(
            new UserFinancialProfileBuilder(Faker)
                .WithUserId(supplier.Id)
                .WithBalance(100m)
                .Build());
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var transaction = new TransactionBuilder(Faker)
            .WithSenderId(supplier.Id)
            .WithReceiverId(systemUser.Id)
            .WithCurrencyId(currency.Id)
            .WithAmount(100m)
            .WithSourceType(TransactionSourceType.Purchase)
            .Completed()
            .Build();

        await _service.ChangeSenderReceiverBalancesAsync(transaction);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        transaction.Reverse(systemUser.Id);
        await _service.ChangeSenderReceiverBalancesAsync(transaction);
        await Context.SaveChangesAsync();

        var supplierProfile = await GetProfile(supplier.Id);
        supplierProfile.Balance.Should().Be(100m);
        (await GetUserBalance(supplier.Id, currency.Id)).Balance.Should().Be(0m);
        (await GetUserBalance(systemUser.Id, currency.Id)).Balance.Should().Be(0m);
    }

    [Fact]
    public async Task ChangeSenderReceiverBalancesAsync_ReverseManualUserToUser_RollsBackBalances()
    {
        var sender = UsersContext.Users.ElementAt(0);
        var receiver = UsersContext.Users.ElementAt(1);
        var reversedBy = UsersContext.Users.ElementAt(2);
        var currency = CurrencyContext.Currencies[0];
        await Context.AddAsync(
            new UserFinancialProfileBuilder(Faker)
                .WithUserId(sender.Id)
                .WithBalance(100m)
                .Build());
        await Context.SaveChangesAsync();

        var transaction = new TransactionBuilder(Faker)
            .WithSenderId(sender.Id)
            .WithReceiverId(receiver.Id)
            .WithCurrencyId(currency.Id)
            .WithAmount(100m)
            .WithSourceType(TransactionSourceType.Manual)
            .Completed()
            .Build();

        await _service.ChangeSenderReceiverBalancesAsync(transaction);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        transaction.Reverse(reversedBy.Id);
        await _service.ChangeSenderReceiverBalancesAsync(transaction);
        await Context.SaveChangesAsync();

        var senderProfile = await GetProfile(sender.Id);
        var receiverProfile = await GetProfile(receiver.Id);
        senderProfile.Balance.Should().Be(100m);
        receiverProfile.Balance.Should().Be(0m);
        (await GetUserBalance(sender.Id, currency.Id)).Balance.Should().Be(0m);
        (await GetUserBalance(receiver.Id, currency.Id)).Balance.Should().Be(0m);
    }

    private Task<UserFinancialProfile> GetProfile(Guid userId)
    {
        return Context.Set<UserFinancialProfile>()
            .AsNoTracking()
            .SingleAsync(x => x.UserId == userId);
    }

    private Task<UserBalance> GetUserBalance(Guid userId, int currencyId)
    {
        return Context.UserBalances
            .AsNoTracking()
            .SingleAsync(x => x.UserId == userId && x.CurrencyId == currencyId);
    }
}