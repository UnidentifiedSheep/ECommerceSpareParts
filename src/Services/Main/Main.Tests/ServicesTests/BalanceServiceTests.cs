using FluentAssertions;
using Main.Application.Interfaces.Services;
using Main.Entities.Balance;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders.Balance;
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
        await Context.AddAsync(new UserFinancialProfileBuilder(Faker)
            .WithUserId(sender.Id)
            .WithWalletBalance(300m)
            .WithSystemBalance(400m)
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
        senderProfile.WalletBalance.Should().Be(0m);
        senderProfile.SystemBalance.Should().Be(0m);
        senderProfile.AvailableBalance.Should().Be(0m);
        receiverProfile.WalletBalance.Should().Be(700m);
        receiverProfile.SystemBalance.Should().Be(0m);

        (await GetUserBalance(sender.Id, currency.Id)).Balance.Should().Be(-700m);
        (await GetUserBalance(receiver.Id, currency.Id)).Balance.Should().Be(700m);
    }

    [Fact]
    public async Task ChangeSenderReceiverBalancesAsync_ManualUserToSystem_PaysSystemDebtFirst()
    {
        var user = UsersContext.Users.ElementAt(0);
        var systemUser = UserContext.SystemUser;
        var currency = CurrencyContext.Currencies[0];
        await Context.AddAsync(new UserFinancialProfileBuilder(Faker)
            .WithUserId(user.Id)
            .WithWalletBalance(300m)
            .WithSystemBalance(-200m)
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
        userProfile.WalletBalance.Should().Be(250m);
        userProfile.SystemBalance.Should().Be(0m);
        userProfile.AvailableBalance.Should().Be(250m);

        (await GetUserBalance(user.Id, currency.Id)).Balance.Should().Be(-250m);
        (await GetUserBalance(systemUser.Id, currency.Id)).Balance.Should().Be(250m);
    }

    [Fact]
    public async Task ChangeSenderReceiverBalancesAsync_ManualSystemToUser_RepaysSystemDebtFirst()
    {
        var user = UsersContext.Users.ElementAt(0);
        var systemUser = UserContext.SystemUser;
        var currency = CurrencyContext.Currencies[0];
        await Context.AddAsync(new UserFinancialProfileBuilder(Faker)
            .WithUserId(user.Id)
            .WithSystemBalance(200m)
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
        userProfile.WalletBalance.Should().Be(50m);
        userProfile.SystemBalance.Should().Be(0m);
        userProfile.AvailableBalance.Should().Be(50m);

        (await GetUserBalance(systemUser.Id, currency.Id)).Balance.Should().Be(-250m);
        (await GetUserBalance(user.Id, currency.Id)).Balance.Should().Be(250m);
    }

    [Theory]
    [InlineData(TransactionSourceType.Purchase)]
    [InlineData(TransactionSourceType.Logistic)]
    public async Task ChangeSenderReceiverBalancesAsync_SystemSettlementUserToSystem_IncreasesUserSystemBalance(
        TransactionSourceType sourceType)
    {
        var user = UsersContext.Users.ElementAt(0);
        var systemUser = UserContext.SystemUser;
        var currency = CurrencyContext.Currencies[0];

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
        userProfile.WalletBalance.Should().Be(0m);
        userProfile.SystemBalance.Should().Be(100m);
        userProfile.AvailableBalance.Should().Be(100m);
    }

    [Fact]
    public async Task ChangeSenderReceiverBalancesAsync_SystemSettlementSystemToUser_DecreasesUserSystemBalance()
    {
        var buyer = UsersContext.Users.ElementAt(0);
        var systemUser = UserContext.SystemUser;
        var currency = CurrencyContext.Currencies[0];

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
        buyerProfile.WalletBalance.Should().Be(0m);
        buyerProfile.SystemBalance.Should().Be(-100m);
        buyerProfile.AvailableBalance.Should().Be(-100m);
    }

    [Fact]
    public async Task ChangeSenderReceiverBalancesAsync_ReverseSystemSettlementUserToSystem_RollsBackUserSystemBalance()
    {
        var supplier = UsersContext.Users.ElementAt(0);
        var systemUser = UserContext.SystemUser;
        var currency = CurrencyContext.Currencies[0];
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
        supplierProfile.WalletBalance.Should().Be(0m);
        supplierProfile.SystemBalance.Should().Be(0m);
        supplierProfile.AvailableBalance.Should().Be(0m);

        (await GetUserBalance(supplier.Id, currency.Id)).Balance.Should().Be(0m);
        (await GetUserBalance(systemUser.Id, currency.Id)).Balance.Should().Be(0m);
    }

    [Fact]
    public async Task ChangeSenderReceiverBalancesAsync_ReverseManualUserToUser_RollsBackWalletBalances()
    {
        var sender = UsersContext.Users.ElementAt(0);
        var receiver = UsersContext.Users.ElementAt(1);
        var reversedBy = UsersContext.Users.ElementAt(2);
        var currency = CurrencyContext.Currencies[0];
        await Context.AddAsync(new UserFinancialProfileBuilder(Faker)
            .WithUserId(sender.Id)
            .WithWalletBalance(100m)
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
        senderProfile.WalletBalance.Should().Be(100m);
        senderProfile.SystemBalance.Should().Be(0m);
        receiverProfile.WalletBalance.Should().Be(0m);
        receiverProfile.SystemBalance.Should().Be(0m);

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
