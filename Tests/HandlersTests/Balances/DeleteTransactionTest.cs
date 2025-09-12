using Application.Configs;
using Application.Handlers.Balance.DeleteTransaction;
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
public class DeleteTransactionTest : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
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
    }
        
    public async Task InitializeAsync()
    {
        await _context.CreateSystemUser();
        await _context.AddMockCurrencies();
        _currency = await _context.Currencies.AsNoTracking().FirstAsync();
        _systemUser = await _context.AspNetUsers.AsNoTracking().FirstAsync(x => x.Id == "SYSTEM");
        var mockUserId = await _mediator.AddMockUser();
        _mockUser = await _context.AspNetUsers.AsNoTracking().FirstAsync(x => x.Id == mockUserId);
        
        //Before test transaction
        await _mediator.AddMockTransaction(_mockUser.Id, _systemUser.Id, _systemUser.Id, 
            100, DateTime.Now.AddMinutes(-1));
        
        _transaction = await _mediator.AddMockTransaction(_mockUser.Id, _systemUser.Id, _systemUser.Id, 
            1200);
        
        //After test transaction
        await _mediator.AddMockTransaction(_mockUser.Id, _systemUser.Id, _systemUser.Id, 
            100, DateTime.Now.AddMinutes(1));

        await _mediator.AddMockProducersAndArticles();
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