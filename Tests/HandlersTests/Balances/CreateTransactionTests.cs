using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.EndPoints.Balances.CreateTransaction;
using MonoliteUnicorn.PostGres.Main;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Balances;

[Collection("Combined collection")]
public class CreateTransactionTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private AspNetUser _mockUser = null!;
    private AspNetUser _systemUser = null!;
    private AspNetUser _adminUser = null!;
    private Currency _currency = null!;

    public CreateTransactionTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }
        
    public async Task InitializeAsync()
    {
        await _context.AddMockProducersAndArticles();
        _systemUser = await _context.CreateSystemUser();
        _mockUser = await _context.AddMockUser();
        _adminUser = await _context.AddMockUser();
        var currencies = await _context.AddMockCurrency(1);
        _currency = currencies.Single();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }
    
    [Fact]
    public async Task CreateOneTransaction_Succeeds()
    {
        var command = new CreateTransactionCommand(
            SenderId: _systemUser.Id,
            ReceiverId: _mockUser.Id,
            Amount: 100,
            CurrencyId: _currency.Id,
            WhoCreatedTransaction: _adminUser.Id,
            TransactionDateTime: DateTime.Now
        );
        
        var result = await _mediator.Send(command);
        
        Assert.Equal(Unit.Value, result);
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(x => x.SenderId == _systemUser.Id && 
                                      x.ReceiverId == _mockUser.Id && 
                                      x.TransactionDatetime == command.TransactionDateTime);
        Assert.NotNull(transaction);
        Assert.Equal(100, transaction.TransactionSum);
    }

    [Fact]
    public async Task CreateTransaction_WithEmptySender_FailsValidation()
    {
        var command = new CreateTransactionCommand(
            SenderId: "",
            ReceiverId: _mockUser.Id,
            Amount: 125,
            CurrencyId: _currency.Id,
            WhoCreatedTransaction: _adminUser.Id,
            TransactionDateTime: DateTime.Now
        );

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateTransaction_WithNegativeAmount_FailsValidation()
    {
        var command = new CreateTransactionCommand(
            SenderId: _systemUser.Id,
            ReceiverId: _mockUser.Id,
            Amount: -100,
            CurrencyId: _currency.Id,
            WhoCreatedTransaction: _adminUser.Id,
            TransactionDateTime: DateTime.Now
        );

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateTransaction_WithOldDate_ThrowsValidationError()
    {
        var command = new CreateTransactionCommand(
            SenderId: _systemUser.Id,
            ReceiverId: _mockUser.Id,
            Amount: 100,
            CurrencyId: _currency.Id,
            WhoCreatedTransaction: _adminUser.Id,
            TransactionDateTime: DateTime.Now.AddMonths(-4)
        );
        
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateTransaction_WithFutureDateBeyondLimit_FailsValidation()
    {
        var command = new CreateTransactionCommand(
            SenderId: _systemUser.Id,
            ReceiverId: _mockUser.Id,
            Amount: 100,
            CurrencyId: _currency.Id,
            WhoCreatedTransaction: _adminUser.Id,
            TransactionDateTime: DateTime.Now.AddMonths(1)
        );
        
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateTransaction_WithEmptyReceiver_FailsValidation()
    {
        var command = new CreateTransactionCommand(
            SenderId: _systemUser.Id,
            ReceiverId: "",
            Amount: 100,
            CurrencyId: _currency.Id,
            WhoCreatedTransaction: _adminUser.Id,
            TransactionDateTime: DateTime.Now
        );

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }
}