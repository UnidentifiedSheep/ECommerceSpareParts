using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions.Balances;
using MonoliteUnicorn.Exceptions.Currencies;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Balances;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.ServicesTests.BalancesTests;

[Collection("Combined collection")]
public class CreateTransactionTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly DContext _context;
    private readonly Faker _faker = new(MockData.MockData.Locale);
    private readonly IBalance _balance;
    
    private Currency _currency = null!;
    private AspNetUser _receiver = null!;
    private AspNetUser _sender = null!;
    
    public CreateTransactionTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _serviceProvider = sp;
        _context = sp.GetRequiredService<DContext>();
        _balance = sp.GetRequiredService<IBalance>();
    }
        
    public async Task InitializeAsync()
    {
        var currencies = await _context.AddMockCurrency(1);
        _currency = currencies.Single();
        _sender = await _context.AddMockUser();
        _receiver = await _context.AddMockUser();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task CreateTransaction_WithInvalidSenderId_ThrowsUserNotFoundException()
    {
        var randomId = _faker.Lorem.Letter(10);
        await Assert.ThrowsAsync<UserNotFoundException>(async () =>
            await _balance.CreateTransactionAsync(randomId, _receiver.Id,
                100, TransactionStatus.Normal, _currency.Id, _sender.Id, DateTime.Now));
    }
    
    [Fact]
    public async Task CreateTransaction_WithInvalidReceiverId_ThrowsUserNotFoundException()
    {
        var randomId = _faker.Lorem.Letter(10);
        await Assert.ThrowsAsync<UserNotFoundException>(async () =>
            await _balance.CreateTransactionAsync(_sender.Id, randomId,
                100, TransactionStatus.Normal, _currency.Id, _sender.Id, DateTime.Now));
    }
    
    [Fact]
    public async Task CreateTransaction_WithInvalidCreatedUserId_ThrowsUserNotFoundException()
    {
        var randomId = _faker.Lorem.Letter(10);
        await Assert.ThrowsAsync<UserNotFoundException>(async () =>
            await _balance.CreateTransactionAsync(_sender.Id, _receiver.Id,
                100, TransactionStatus.Normal, _currency.Id, randomId, DateTime.Now));
    }
    
    [Fact]
    public async Task CreateTransaction_WithInvalidCurrencyId_ThrowsCurrencyNotFoundException()
    {
        await Assert.ThrowsAsync<CurrencyNotFoundException>(async () =>
            await _balance.CreateTransactionAsync(_sender.Id, _receiver.Id,
                100, TransactionStatus.Normal, _faker.Random.Int(9999), _sender.Id, DateTime.Now));
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(0.0001)]
    [InlineData(-1)]
    [InlineData(-99999999.01)]
    public async Task CreateTransaction_WithInvalidAmount_ThrowsZeroOrNegativeAmountException(decimal amount)
    {
        await Assert.ThrowsAsync<ZeroOrNegativeTransactionAmountException>(async () =>
            await _balance.CreateTransactionAsync(_sender.Id, _receiver.Id,
                amount, TransactionStatus.Normal, _currency.Id, _sender.Id, DateTime.Now));
    }
    
    [Fact]
    public async Task CreateTransaction_WithSameSenderAndReceiver_ThrowsSameSenderAndReceiverException()
    {
        await Assert.ThrowsAsync<SameSenderAndReceiverException>(async () =>
            await _balance.CreateTransactionAsync(_sender.Id, _sender.Id,
                100, TransactionStatus.Normal, _currency.Id, _sender.Id, DateTime.Now));
    }
    
    [Fact]
    public async Task CreateTransaction_WithSameTransactionData_ThrowsSameTransactionExists()
    {
        var time = DateTime.Now;
        await _balance.CreateTransactionAsync(_sender.Id, _receiver.Id,
            100, TransactionStatus.Normal, _currency.Id, _sender.Id, time);
        await Assert.ThrowsAsync<SameTransactionExists>(async () =>
            await _balance.CreateTransactionAsync(_sender.Id, _receiver.Id,
                100, TransactionStatus.Normal, _currency.Id, _sender.Id, time));
    }
    
    [Fact]
    public async Task CreateTransaction_WithNormalData_Succeeds()
    {
        var time = DateTime.Now;
        var senderBalanceBefore = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _sender.Id && x.CurrencyId == _currency.Id))?.Balance ?? 0;
        var receiverBalanceBefore = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _receiver.Id && x.CurrencyId == _currency.Id))?.Balance ?? 0;

        await _balance.CreateTransactionAsync(_sender.Id, _receiver.Id,
            100, TransactionStatus.Normal, _currency.Id, _sender.Id, time);
        
        var senderBalanceAfter = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _sender.Id && x.CurrencyId == _currency.Id))?.Balance;
        var receiverBalanceAfter = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _receiver.Id && x.CurrencyId == _currency.Id))?.Balance;
        
        Assert.Equal(senderBalanceBefore - 100, senderBalanceAfter);
        Assert.Equal(receiverBalanceBefore + 100, receiverBalanceAfter);
        
        var transaction = await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SenderId == _sender.Id 
                                      && x.ReceiverId == _receiver.Id
                                      && x.CurrencyId == _currency.Id
                                      && x.TransactionSum == 100
                                      && x.TransactionDatetime == time);
        Assert.NotNull(transaction);
    }
    
    [Fact]
    public async Task CreateTransaction_WithNormalDataTwoTransactionInARow_Succeeds()
    {
        var time = DateTime.Now;
        var senderBalanceBefore = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _sender.Id && x.CurrencyId == _currency.Id))?.Balance ?? 0;
        var receiverBalanceBefore = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _receiver.Id && x.CurrencyId == _currency.Id))?.Balance ?? 0;

        await _balance.CreateTransactionAsync(_sender.Id, _receiver.Id,
            100, TransactionStatus.Normal, _currency.Id, _sender.Id, time);
        
        await _balance.CreateTransactionAsync(_sender.Id, _receiver.Id,
            100, TransactionStatus.Normal, _currency.Id, _sender.Id, time.AddMinutes(1));
        
        var senderBalanceAfter = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _sender.Id && x.CurrencyId == _currency.Id))?.Balance;
        var receiverBalanceAfter = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _receiver.Id && x.CurrencyId == _currency.Id))?.Balance;
        
        Assert.Equal(senderBalanceBefore - 200, senderBalanceAfter);
        Assert.Equal(receiverBalanceBefore + 200, receiverBalanceAfter);
        
        var firstTransaction = await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SenderId == _sender.Id 
                                      && x.ReceiverId == _receiver.Id
                                      && x.CurrencyId == _currency.Id
                                      && x.TransactionSum == 100
                                      && x.TransactionDatetime == time);
        
        var secondTransaction = await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SenderId == _sender.Id 
                                      && x.ReceiverId == _receiver.Id
                                      && x.CurrencyId == _currency.Id
                                      && x.TransactionSum == 100
                                      && x.TransactionDatetime == time.AddMinutes(1));
        Assert.NotNull(firstTransaction);
        Assert.NotNull(secondTransaction);
    }
    
    [Fact]
    public async Task CreateTransaction_WithNormalDataInParallel_Succeeds()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var balanceService = scope.ServiceProvider.GetRequiredService<IBalance>();
        var time = DateTime.Now;
        var senderBalanceBefore = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _sender.Id && x.CurrencyId == _currency.Id))?.Balance ?? 0;
        var receiverBalanceBefore = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _receiver.Id && x.CurrencyId == _currency.Id))?.Balance ?? 0;

        var task1 = _balance.CreateTransactionAsync(_sender.Id, _receiver.Id,
            100, TransactionStatus.Normal, _currency.Id, _sender.Id, time);
        
        var task2 = balanceService.CreateTransactionAsync(_sender.Id, _receiver.Id,
            100, TransactionStatus.Normal, _currency.Id, _sender.Id, time.AddMinutes(1));
        
        await Task.WhenAll(task1, task2);
        var senderBalanceAfter = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _sender.Id && x.CurrencyId == _currency.Id))?.Balance;
        var receiverBalanceAfter = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _receiver.Id && x.CurrencyId == _currency.Id))?.Balance;
        
        Assert.Equal(senderBalanceBefore - 200, senderBalanceAfter);
        Assert.Equal(receiverBalanceBefore + 200, receiverBalanceAfter);
        
        var firstTransaction = await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SenderId == _sender.Id 
                                      && x.ReceiverId == _receiver.Id
                                      && x.CurrencyId == _currency.Id
                                      && x.TransactionSum == 100
                                      && x.TransactionDatetime == time);
        
        var secondTransaction = await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SenderId == _sender.Id 
                                      && x.ReceiverId == _receiver.Id
                                      && x.CurrencyId == _currency.Id
                                      && x.TransactionSum == 100
                                      && x.TransactionDatetime == time.AddMinutes(1));
        Assert.NotNull(firstTransaction);
        Assert.NotNull(secondTransaction);
    }
    [Fact]
    public async Task CreateTransaction_WithNormalDataManyTransactions_Succeeds()
    {
        var initTime = DateTime.Now;
        var time = initTime;
        var senderBalanceBefore = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _sender.Id && x.CurrencyId == _currency.Id))?.Balance ?? 0;
        var receiverBalanceBefore = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _receiver.Id && x.CurrencyId == _currency.Id))?.Balance ?? 0;

        decimal totalTransactionsSum = 0;
        for (int i = 0; i < 10; i++)
        {
            time = time.AddMinutes(1);
            var amount = Math.Round(_faker.Random.Decimal(0.01m, 1000000), 2);
            totalTransactionsSum += amount;
            await _balance.CreateTransactionAsync(_sender.Id, _receiver.Id,
                amount, TransactionStatus.Normal, _currency.Id, _sender.Id, time);
        }
        
        
        var senderBalanceAfter = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _sender.Id && x.CurrencyId == _currency.Id))?.Balance;
        var receiverBalanceAfter = (await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _receiver.Id && x.CurrencyId == _currency.Id))?.Balance;
        
        Assert.Equal(senderBalanceBefore - totalTransactionsSum, senderBalanceAfter);
        Assert.Equal(receiverBalanceBefore + totalTransactionsSum, receiverBalanceAfter);

        var transactionsBefore = await _context.Transactions
            .AsNoTracking()
            .OrderByDescending(x => x.TransactionDatetime)
            .ToListAsync();
        
        var transactionInPast = await _balance.CreateTransactionAsync(_sender.Id, _receiver.Id,
            100, TransactionStatus.Normal, _currency.Id, _sender.Id, initTime.AddMinutes(-1));
        
        var transactionsAfter = await _context.Transactions
            .AsNoTracking()
            .Where(x => x.Id != transactionInPast.Id)
            .OrderByDescending(x => x.TransactionDatetime)
            .ToDictionaryAsync(x => x.Id);

        foreach (var before in transactionsBefore)
        {
            var after = transactionsAfter[before.Id];
            Assert.Equal(before.SenderBalanceAfterTransaction - 100, after.SenderBalanceAfterTransaction);
            Assert.Equal(before.ReceiverBalanceAfterTransaction + 100, after.ReceiverBalanceAfterTransaction);
        }
        
    }
    
    
}