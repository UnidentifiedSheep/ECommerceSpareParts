using Exceptions;
using FluentAssertions;
using Main.Application.Interfaces.Services;
using Main.Entities.Balance;
using Main.Entities.Organization;
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
        RegisterBasicContext<CurrencyRatesTestContext>();
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
    public async Task GetBalanceInBaseCurrencyAsync_SumsBalancesInAllCurrencies()
    {
        var organizationId = UsersContext.Users.First().Id;
        var baseCurrency = CurrencyContext.Currencies[0];
        var foreignCurrency = CurrencyContext.Currencies[1];
        var foreignRate = GetContext<CurrencyRatesTestContext>().Rates
            .Single(x => x.FromCurrencyId == foreignCurrency.Id)
            .Rate;
        var baseBalance = OrganizationBalance.Create(organizationId, baseCurrency.Id);
        var foreignBalance = OrganizationBalance.Create(organizationId, foreignCurrency.Id);
        baseBalance.IncrementBalance(25m);
        foreignBalance.IncrementBalance(100m * foreignRate);
        await Context.AddRangeAsync(baseBalance, foreignBalance);
        await Context.SaveChangesAsync();

        var result = await _service.GetBalanceInBaseCurrencyAsync(organizationId);

        result.Should().Be(125m);
    }

    [Fact]
    public async Task ChangeSenderReceiverBalancesAsync_AllCurrencyBalanceWouldFallBelowMinimum_Throws()
    {
        var sender = UsersContext.Users.ElementAt(0);
        var receiver = UsersContext.Users.ElementAt(1);
        var baseCurrency = CurrencyContext.Currencies[0];
        var foreignCurrency = CurrencyContext.Currencies[1];
        var foreignRate = GetContext<CurrencyRatesTestContext>().Rates
            .Single(x => x.FromCurrencyId == foreignCurrency.Id)
            .Rate;
        var receiverBalance = OrganizationBalance.Create(receiver.Id, foreignCurrency.Id);
        receiverBalance.IncrementBalance(100m * foreignRate);
        var receiverProfile = OrganizationFinancialProfile.Create(receiver.Id, -40m);
        await Context.AddRangeAsync(receiverBalance, receiverProfile);
        await Context.SaveChangesAsync();
        var transaction = new TransactionBuilder(Faker)
            .WithSenderId(sender.Id)
            .WithReceiverId(receiver.Id)
            .WithCurrencyId(baseCurrency.Id)
            .WithAmount(150m)
            .Completed()
            .Build();

        var act = () => _service.ChangeSenderReceiverBalancesAsync(transaction);

        await act.Should().ThrowAsync<InvalidInputException>();
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

        await _service.ChangeSenderReceiverBalancesAsync(transaction, true);
        await Context.SaveChangesAsync();

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

        await _service.ChangeSenderReceiverBalancesAsync(transaction, true);
        await Context.SaveChangesAsync();

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

        await _service.ChangeSenderReceiverBalancesAsync(transaction, true);
        await Context.SaveChangesAsync();

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

        await _service.ChangeSenderReceiverBalancesAsync(transaction, true);
        await Context.SaveChangesAsync();

        (await GetUserBalance(user.Id, currency.Id)).Balance.Should().Be(100m);
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

        await _service.ChangeSenderReceiverBalancesAsync(transaction, true);
        await Context.SaveChangesAsync();

        (await GetUserBalance(buyer.Id, currency.Id)).Balance.Should().Be(-100m);
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

        await _service.ChangeSenderReceiverBalancesAsync(transaction, true);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        transaction.Reverse(systemUser.Id);
        await _service.ChangeSenderReceiverBalancesAsync(transaction);
        await Context.SaveChangesAsync();

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

        await _service.ChangeSenderReceiverBalancesAsync(transaction, true);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        transaction.Reverse(reversedBy.Id);
        await _service.ChangeSenderReceiverBalancesAsync(transaction);
        await Context.SaveChangesAsync();

        (await GetUserBalance(sender.Id, currency.Id)).Balance.Should().Be(0m);
        (await GetUserBalance(receiver.Id, currency.Id)).Balance.Should().Be(0m);
    }

    private Task<OrganizationBalance> GetUserBalance(Guid userId, int currencyId)
    {
        return Context.UserBalances
            .AsNoTracking()
            .SingleAsync(x => x.OrganizationId == userId && x.CurrencyId == currencyId);
    }
}
