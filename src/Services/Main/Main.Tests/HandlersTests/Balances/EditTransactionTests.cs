using Exceptions.Exceptions.Balances;
using FluentValidation;
using Main.Application.Configs;
using Main.Application.Handlers.Balance.EditTransaction;
using Main.Entities;
using Main.Enums;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Balances;

[Collection("Combined collection")]
public class EditTransactionTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    private Currency _currency = null!;
    private User _receiver = null!;
    private User _sender = null!;
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
        var systemId = await _mediator.AddMockUser();
        await _context.AddMockCurrencies();
        _currency = await _context.Currencies.AsNoTracking().FirstAsync();
        _sender = await _context.Users.AsNoTracking().FirstAsync(x => x.Id == systemId);
        var mockUserId = await _mediator.AddMockUser();
        _receiver = await _context.Users.AsNoTracking().FirstAsync(x => x.Id == mockUserId);

        for (var i = 0; i < 10; i++)
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
        var newDate = DateTime.Now.AddDays(-1);
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
            if (!balancesDict.TryGetValue(tr.SenderId.ToString() + tr.CurrencyId, out var senderBalance))
                senderBalance = 0;
            if (!balancesDict.TryGetValue(tr.ReceiverId.ToString() + tr.CurrencyId, out var receiverBalance))
                receiverBalance = 0;
            var amount = tr.TransactionSum;
            balancesDict[tr.SenderId.ToString() + tr.CurrencyId] = senderBalance - amount;
            balancesDict[tr.ReceiverId.ToString() + tr.CurrencyId] = receiverBalance + amount;
            Assert.Equal(balancesDict[tr.SenderId.ToString() + tr.CurrencyId], tr.SenderBalanceAfterTransaction);
            Assert.Equal(balancesDict[tr.ReceiverId.ToString() + tr.CurrencyId], tr.ReceiverBalanceAfterTransaction);
        }

        Assert.False(updatedTransaction.IsDeleted);
        //balances assert
        Assert.Equal(senderBalanceBeforeFirstCurrency.Balance + transaction.TransactionSum,
            senderBalanceAfterFirstCurrency.Balance);
        Assert.Equal(receiverBalanceBeforeFirstCurrency.Balance - transaction.TransactionSum,
            receiverBalanceAfterFirstCurrency.Balance);
        Assert.NotNull(senderBalanceAfterSecondCurrency);
        Assert.NotNull(receiverBalanceAfterSecondCurrency);
        Assert.Equal(-newAmount, senderBalanceAfterSecondCurrency.Balance);
        Assert.Equal(newAmount, receiverBalanceAfterSecondCurrency.Balance);

        Assert.NotNull(updatedTransaction);
        Assert.Equal(newCurrency.Id, updatedTransaction.CurrencyId);
        Assert.Equal(newAmount, updatedTransaction.TransactionSum);
        Assert.Equal(newStatus, updatedTransaction.Status);
        Assert.True(Math.Abs((updatedTransaction.TransactionDatetime - newDate.ToUniversalTime()).TotalMilliseconds) < 1);

        var version = await _context.TransactionVersions
            .Where(x => x.TransactionId == transaction.Id)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync();

        Assert.NotNull(version);
        Assert.Equal(transaction.TransactionSum, version.TransactionSum);
        Assert.Equal(transaction.Status, version.Status);
        Assert.True(Math.Abs((transaction.TransactionDatetime.ToUniversalTime() - version.TransactionDatetime.ToUniversalTime()).TotalMilliseconds) < 1);
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
        var newDate = transaction.TransactionDatetime.AddSeconds(-1);

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
        Assert.Equal(newStatus, updatedTransaction.Status);
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
        Assert.True(Math.Abs((version.TransactionDatetime.ToUniversalTime() - oldDate.ToUniversalTime()).TotalSeconds) < 1);
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task EditTransaction_WhenAmountIsZeroOrNegative_ThrowsZeroOrNegativeTransactionAmountException(
        decimal amount)
    {
        var transaction = _transactions.First();

        var command = new EditTransactionCommand(transaction.Id, transaction.CurrencyId, amount,
            TransactionStatus.Normal, DateTime.Now);

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditTransaction_WhenTransactionIsDeleted_ThrowsEditingDeletedTransactionException()
    {
        var transaction = _transactions.First();
        transaction.IsDeleted = true;
        await _context.SaveChangesAsync();

        var command = new EditTransactionCommand(transaction.Id, transaction.CurrencyId, 123.45m,
            TransactionStatus.Normal, DateTime.Now);

        await Assert.ThrowsAsync<TransactionNotFoundExcpetion>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditTransaction_WithInvalidTransactionId_ThrowsTransactionNotFoundException()
    {
        var command =
            new EditTransactionCommand(Guid.NewGuid(), _currency.Id, 100m, TransactionStatus.Normal, DateTime.Now);

        await Assert.ThrowsAsync<TransactionNotFoundExcpetion>(async () => await _mediator.Send(command));
    }
}