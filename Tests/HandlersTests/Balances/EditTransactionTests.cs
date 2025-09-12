using Application.Configs;
using Application.Handlers.Balance.EditTransaction;
using Core.Entities;
using Core.Enums;
using Exceptions.Exceptions.Balances;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Balances;

[Collection("Combined collection")]
public class EditTransactionTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    
    private Currency _currency = null!;
    private AspNetUser _receiver = null!;
    private AspNetUser _sender = null!;
    private List<Transaction> _transactions = null!;
    
    public EditTransactionTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _mediator = sp.GetRequiredService<IMediator>();
    }
        
    public async Task InitializeAsync()
    {
        await _context.CreateSystemUser();
        await _context.AddMockCurrencies();
        _currency = await _context.Currencies.AsNoTracking().FirstAsync();
        _sender = await _context.AspNetUsers.AsNoTracking().FirstAsync(x => x.Id == "SYSTEM");
        var mockUserId = await _mediator.AddMockUser();
        _receiver = await _context.AspNetUsers.AsNoTracking().FirstAsync(x => x.Id == mockUserId);

        for (int i = 0; i < 10; i++)
            await _mediator.AddMockTransaction(_sender.Id, _receiver.Id, _sender.Id);
        
        _transactions = await _context.Transactions.ToListAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task EditTransaction_NormalWithCurrencyChange_Succeeds()
    {
        var transaction = await _context.Transactions
            .AsNoTracking()
            .OrderBy(x => x.TransactionDatetime)
            .ThenBy(x => x.Id)
            .Skip(5)
            .FirstAsync();
        
        var newAmount = 999.99m;
        var newStatus = TransactionStatus.Purchase;
        var newDate = DateTime.SpecifyKind(DateTime.Now.AddDays(-1), DateTimeKind.Unspecified);
        var newCurrency = await _context.Currencies.AsNoTracking().FirstAsync(x => x.Id != _currency.Id);
        
        var senderBalanceBeforeFirstCurrency = await _context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == transaction.SenderId && x.CurrencyId == transaction.CurrencyId);
        var receiverBalanceBeforeFirstCurrency = await _context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == transaction.ReceiverId && x.CurrencyId == transaction.CurrencyId);
        
        var command = new EditTransactionCommand(transaction.Id, newCurrency.Id, newAmount, newStatus, newDate);
        await _mediator.Send(command);

        var updatedTransaction = await _context.Transactions
            .AsNoTracking()
            .FirstAsync(x => x.Id == transaction.Id);
        
        var transactionsAfter = await _context.Transactions
            .AsNoTracking()
            .OrderBy(x => x.TransactionDatetime)
            .ToListAsync();
        
        var senderBalanceAfterFirstCurrency = await _context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == transaction.SenderId && x.CurrencyId == transaction.CurrencyId);
        var receiverBalanceAfterFirstCurrency = await _context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == transaction.ReceiverId && x.CurrencyId == transaction.CurrencyId);
        
        var senderBalanceAfterSecondCurrency = await _context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == transaction.SenderId && x.CurrencyId == updatedTransaction.CurrencyId);
        var receiverBalanceAfterSecondCurrency = await _context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == transaction.ReceiverId && x.CurrencyId == updatedTransaction.CurrencyId);

        var balancesDict = new Dictionary<string, decimal>();
        foreach (var tr in transactionsAfter)
        {
            if (!balancesDict.TryGetValue(tr.SenderId + tr.CurrencyId, out var senderBalance))
                senderBalance = 0;
            if (!balancesDict.TryGetValue(tr.ReceiverId + tr.CurrencyId, out var receiverBalance))
                receiverBalance = 0;
            var amount = tr.TransactionSum;
            balancesDict[tr.SenderId + tr.CurrencyId] = senderBalance - amount;
            balancesDict[tr.ReceiverId + tr.CurrencyId] = receiverBalance + amount;
            Assert.Equal(balancesDict[tr.SenderId + tr.CurrencyId], tr.SenderBalanceAfterTransaction);
            Assert.Equal(balancesDict[tr.ReceiverId + tr.CurrencyId], tr.ReceiverBalanceAfterTransaction);
        }
        Assert.False(updatedTransaction.IsDeleted);
        //balances assert
        Assert.Equal(senderBalanceBeforeFirstCurrency.Balance + transaction.TransactionSum, senderBalanceAfterFirstCurrency.Balance);
        Assert.Equal(receiverBalanceBeforeFirstCurrency.Balance - transaction.TransactionSum, receiverBalanceAfterFirstCurrency.Balance);
        Assert.NotNull(senderBalanceAfterSecondCurrency);
        Assert.NotNull(receiverBalanceAfterSecondCurrency);
        Assert.Equal(-newAmount, senderBalanceAfterSecondCurrency.Balance);
        Assert.Equal(newAmount, receiverBalanceAfterSecondCurrency.Balance);
        
        Assert.NotNull(updatedTransaction);
        Assert.Equal(newCurrency.Id, updatedTransaction.CurrencyId);
        Assert.Equal(newAmount, updatedTransaction.TransactionSum);
        Assert.Equal(newStatus.ToString(), updatedTransaction.Status);
        Assert.True(Math.Abs((updatedTransaction.TransactionDatetime - newDate).TotalMilliseconds) < 1);

        var version = await _context.TransactionVersions
            .Where(x => x.TransactionId == transaction.Id)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync();

        Assert.NotNull(version);
        Assert.Equal(transaction.TransactionSum, version.TransactionSum);
        Assert.Equal(transaction.Status, version.Status);
        Assert.True(Math.Abs((transaction.TransactionDatetime - version.TransactionDatetime).TotalMilliseconds) < 1);
    }
    
    [Fact]
    public async Task EditTransaction_NormalWithOutCurrencyChange_Succeeds()
    {
        // Arrange
        var transaction = await _context.Transactions
            .AsNoTracking()
            .OrderBy(x => x.TransactionDatetime)
            .ThenBy(x => x.Id)
            .Skip(5)
            .FirstAsync();

        var oldAmount = transaction.TransactionSum;
        var oldStatus = transaction.Status;
        var oldDate = transaction.TransactionDatetime;

        var newAmount = 999.99m;
        var newStatus = TransactionStatus.Purchase;
        var newDate = DateTime.SpecifyKind(transaction.TransactionDatetime.AddSeconds(-1), DateTimeKind.Unspecified);

        var senderBalanceBefore = await _context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == transaction.SenderId && x.CurrencyId == transaction.CurrencyId);

        var receiverBalanceBefore = await _context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == transaction.ReceiverId && x.CurrencyId == transaction.CurrencyId);

        var command = new EditTransactionCommand(transaction.Id, transaction.CurrencyId, newAmount, newStatus, newDate);
        await _mediator.Send(command);

        var updatedTransaction = await _context.Transactions
            .AsNoTracking()
            .FirstAsync(x => x.Id == transaction.Id);

        Assert.NotNull(updatedTransaction);
        Assert.False(updatedTransaction.IsDeleted);
        Assert.Equal(newAmount, updatedTransaction.TransactionSum);
        Assert.Equal(newStatus.ToString(), updatedTransaction.Status);
        Assert.True(Math.Abs((updatedTransaction.TransactionDatetime - newDate).TotalSeconds) < 1);

        var senderBalanceAfter = await _context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == transaction.SenderId && x.CurrencyId == transaction.CurrencyId);

        var receiverBalanceAfter = await _context.UserBalances
            .AsNoTracking()
            .FirstAsync(x => x.UserId == transaction.ReceiverId && x.CurrencyId == transaction.CurrencyId);

        Assert.Equal(senderBalanceBefore.Balance + oldAmount - newAmount, senderBalanceAfter.Balance);
        Assert.Equal(receiverBalanceBefore.Balance - oldAmount + newAmount, receiverBalanceAfter.Balance);

        var allTransactions = await _context.Transactions
            .AsNoTracking()
            .OrderBy(x => x.TransactionDatetime)
            .ThenBy(x => x.Id)
            .ToListAsync();

        var balances = new Dictionary<string, decimal>();

        foreach (var tr in allTransactions)
        {
            if (tr.IsDeleted) continue;

            var senderKey = $"{tr.SenderId}-{tr.CurrencyId}";
            var receiverKey = $"{tr.ReceiverId}-{tr.CurrencyId}";

            if (!balances.TryGetValue(senderKey, out var senderBalance))
                senderBalance = 0;
            if (!balances.TryGetValue(receiverKey, out var receiverBalance))
                receiverBalance = 0;

            balances[senderKey] = senderBalance - tr.TransactionSum;
            balances[receiverKey] = receiverBalance + tr.TransactionSum;

            Assert.Equal(balances[senderKey], tr.SenderBalanceAfterTransaction);
            Assert.Equal(balances[receiverKey], tr.ReceiverBalanceAfterTransaction);
        }

        var version = await _context.TransactionVersions
            .Where(x => x.TransactionId == transaction.Id)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync();

        Assert.NotNull(version);
        Assert.Equal(oldAmount, version.TransactionSum);
        Assert.Equal(oldStatus, version.Status);
        Assert.True(Math.Abs((version.TransactionDatetime - oldDate).TotalSeconds) < 1);
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task EditTransaction_WhenAmountIsZeroOrNegative_ThrowsZeroOrNegativeTransactionAmountException(decimal amount)
    {
        var transaction = _transactions.First();
        var invalidAmount = amount;
        
        var command = new EditTransactionCommand(transaction.Id, transaction.CurrencyId, invalidAmount,  TransactionStatus.Normal, DateTime.Now);
        
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditTransaction_WhenTransactionIsDeleted_ThrowsEditingDeletedTransactionException()
    {
        var transaction = _transactions.First();
        transaction.IsDeleted = true;
        await _context.SaveChangesAsync();

        var command = new EditTransactionCommand(transaction.Id, transaction.CurrencyId, 123.45m,  TransactionStatus.Normal, DateTime.Now);
        
        await Assert.ThrowsAsync<TransactionNotFound>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditTransaction_WithInvalidTransactionId_ThrowsTransactionNotFoundException()
    {
        var nonExistentId = Guid.NewGuid().ToString();
        var command = new EditTransactionCommand(nonExistentId, _currency.Id, 100m, TransactionStatus.Normal, DateTime.Now);

        await Assert.ThrowsAsync<TransactionNotFound>(async () => await _mediator.Send(command));
    }
}