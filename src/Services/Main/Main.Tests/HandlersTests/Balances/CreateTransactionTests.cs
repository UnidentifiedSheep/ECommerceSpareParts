using Main.Application.Configs;
using Main.Application.Handlers.Balance.CreateTransaction;
using Core.Entities;
using Core.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Balances;

[Collection("Combined collection")]
public class CreateTransactionTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private User _adminUser = null!;
    private Currency _currency = null!;
    private User _mockUser = null!;
    private User _systemUser = null!;

    public CreateTransactionTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockProducersAndArticles();
        await _context.AddMockCurrencies();
        var systemId = await _mediator.AddMockUser();
        _systemUser = await _context.Users.AsNoTracking().FirstAsync(x => x.Id == systemId);
        await _mediator.AddMockUser();
        await _mediator.AddMockUser();
        _mockUser = await _context.Users.AsNoTracking().FirstAsync(x => x.Id != systemId);
        _adminUser = await _context.Users.AsNoTracking()
            .FirstAsync(x => x.Id != systemId && x.Id != _mockUser.Id);
        _currency = await _context.Currencies.AsNoTracking().FirstAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task CreateOneTransaction_Succeeds()
    {
        var command = new CreateTransactionCommand(
            _systemUser.Id,
            _mockUser.Id,
            100,
            _currency.Id,
            _adminUser.Id,
            DateTime.Now,
            TransactionStatus.Normal
        );

        var createdTransaction = (await _mediator.Send(command)).Transaction;

        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(x => x.SenderId == _systemUser.Id &&
                                      x.ReceiverId == _mockUser.Id &&
                                      x.TransactionDatetime == command.TransactionDateTime);
        Assert.NotNull(transaction);
        Assert.Equal(createdTransaction.TransactionSum, transaction.TransactionSum);
    }

    [Fact]
    public async Task CreateTransaction_WithEmptySender_FailsValidation()
    {
        var command = new CreateTransactionCommand(
            Guid.Empty,
            _mockUser.Id,
            125,
            _currency.Id,
            _adminUser.Id,
            DateTime.Now,
            TransactionStatus.Normal
        );

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateTransaction_WithNegativeAmount_FailsValidation()
    {
        var command = new CreateTransactionCommand(
            _systemUser.Id,
            _mockUser.Id,
            -100,
            _currency.Id,
            _adminUser.Id,
            DateTime.Now,
            TransactionStatus.Normal
        );

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateTransaction_WithOldDate_ThrowsValidationError()
    {
        var command = new CreateTransactionCommand(
            _systemUser.Id,
            _mockUser.Id,
            100,
            _currency.Id,
            _adminUser.Id,
            DateTime.Now.AddMonths(-4),
            TransactionStatus.Normal
        );

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateTransaction_WithFutureDateBeyondLimit_FailsValidation()
    {
        var command = new CreateTransactionCommand(
            _systemUser.Id,
            _mockUser.Id,
            100,
            _currency.Id,
            _adminUser.Id,
            DateTime.Now.AddMonths(1),
            TransactionStatus.Normal
        );

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateTransaction_WithEmptyReceiver_FailsValidation()
    {
        var command = new CreateTransactionCommand(
            _systemUser.Id,
            Guid.Empty,
            100,
            _currency.Id,
            _adminUser.Id,
            DateTime.Now,
            TransactionStatus.Normal
        );

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10000000)]
    [InlineData(0.001)]
    [InlineData(0.0001)]
    [InlineData(0.009)]
    public async Task CreateTransaction_InvalidAmount_ThrowsValidationException(decimal amount)
    {
        var command = new CreateTransactionCommand(
            _systemUser.Id,
            _mockUser.Id,
            amount,
            _currency.Id,
            _adminUser.Id,
            DateTime.Now,
            TransactionStatus.Normal
        );

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }
}