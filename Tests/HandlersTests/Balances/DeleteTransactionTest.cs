using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.EndPoints.Balances.DeleteTransaction;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions.Balances;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Balances;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Balances;

[Collection("Combined collection")]
public class DeleteTransactionTest : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private readonly IBalance _balance;
    private Transaction _transaction = null!;
    private Currency _currency = null!;
    private AspNetUser _systemUser = null!;
    private AspNetUser _mockUser = null!;
    
    public DeleteTransactionTest(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
        _balance = sp.GetRequiredService<IBalance>();
    }
        
    public async Task InitializeAsync()
    {
        _currency = await _context.AddMockCurrency();
        _systemUser = await _context.CreateSystemUser();
        _mockUser = await _context.AddMockUser();
        
        //Before test transaction
        await _balance.CreateTransactionAsync(_mockUser.Id, _systemUser.Id, 100, 
            TransactionStatus.Normal, _currency.Id, _systemUser.Id, DateTime.Now.AddMinutes(-1));
        
        _transaction = await _balance.CreateTransactionAsync(_mockUser.Id, _systemUser.Id, 1200, 
            TransactionStatus.Normal, _currency.Id, _systemUser.Id, DateTime.Now);
        
        //After test transaction
        await _balance.CreateTransactionAsync(_mockUser.Id, _systemUser.Id, 100, 
            TransactionStatus.Normal, _currency.Id, _systemUser.Id, DateTime.Now.AddMinutes(1));
        
        await _context.AddMockProducersAndArticles();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task DeleteTransaction_EmptyTransactionId_FailsValidation()
    {
        var command = new DeleteTransactionCommand("  ", _mockUser.Id);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task DeleteTransaction_EmptyUserId_FailsValidation()
    {
        var command = new DeleteTransactionCommand(_transaction.Id, "  ");
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task DeleteTransaction_Normal_Succeeds()
    {
        var command = new DeleteTransactionCommand(_transaction.Id, _mockUser.Id);
        var result = await _mediator.Send(command);
        Assert.Equal(result, Unit.Value);
        
        var transactions = await _context.Transactions
            .AsNoTracking()
            .IgnoreQueryFilters()
            .ToListAsync();
        var transaction = transactions.FirstOrDefault(x => x.Id == _transaction.Id);
        var notDeletedTransactions = transactions.Where(x => !x.IsDeleted).ToList();
        
        Assert.NotNull(transaction);
        Assert.True(transaction.IsDeleted);
        Assert.Equal(2, notDeletedTransactions.Count);
        
        var senderBalance = await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _mockUser.Id && 
                                      x.CurrencyId == _currency.Id);
        var receiverBalance = await _context.UserBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _systemUser.Id && 
                                      x.CurrencyId == _currency.Id);
        
        Assert.NotNull(senderBalance);
        Assert.NotNull(receiverBalance);
        
        Assert.Equal(-200, senderBalance.Balance);
        Assert.Equal(200, receiverBalance.Balance);
    }

    [Fact]
    public async Task DeleteTransaction_NotNormalStatus_ThrowBadTransactionStatusException()
    {
        _transaction.Status = nameof(TransactionStatus.Purchase);
        await _context.SaveChangesAsync();
        
        var command = new DeleteTransactionCommand(_transaction.Id, _mockUser.Id);
        await Assert.ThrowsAsync<BadTransactionStatusException>(async () => await _mediator.Send(command));
    }
}