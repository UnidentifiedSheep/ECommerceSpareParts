using FluentValidation;
using Main.Abstractions.Consts;
using Main.Application.Handlers.Purchases.CreatePurchase;
using Main.Abstractions.Dtos.Amw.Purchase;
using Main.Entities;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.Purchases;

[Collection("Combined collection")]
public class CreatePurchaseTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    private Currency _currency = null!;
    private Storage _storage = null!;
    private Transaction _transaction = null!;
    private User _user = null!;
    private Article _article = null!;

    public CreatePurchaseTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockUser();
        await _mediator.AddMockUser();
        await _mediator.AddMockProducersAndArticles();
        await _mediator.AddMockStorage();
        await _context.AddMockCurrencies();

        _user = await _context.Users.FirstAsync();
        _storage = await _context.Storages.FirstAsync();
        _article = await _context.Articles.FirstAsync();
        _currency = await _context.Currencies.FirstAsync();

        var receiver = await _context.Users.FirstAsync(x => x.Id != _user.Id);
        await _mediator.AddMockTransaction(_user.Id, receiver.Id, _user.Id, 1000m);
        _transaction = await _context.Transactions.FirstAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task CreatePurchase_WithValidData_Succeeds()
    {
        var content = new List<NewPurchaseContentDto>
        {
            new()
            {
                ArticleId = _article.Id,
                Count = 10,
                Price = 100.50m
            }
        };

        var command = new CreatePurchaseCommand(content, _currency.Id, "Test Comment",
            _user.Id, _transaction.Id, _storage.Name, _user.Id, DateTime.Now);

        var result = await _mediator.Send(command);

        Assert.NotNull(result.Purchase);
        Assert.Equal(_currency.Id, result.Purchase.CurrencyId);
        Assert.Equal("Test Comment", result.Purchase.Comment);
        Assert.Equal(_storage.Name, result.Purchase.Storage);
        Assert.Single(result.Purchase.PurchaseContents);
    }

    [Fact]
    public async Task CreatePurchase_WithEmptyContent_ThrowsValidationException()
    {
        var command = new CreatePurchaseCommand([], _currency.Id, "Test Comment",
            _user.Id, _transaction.Id, _storage.Name, _user.Id, DateTime.Now);

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreatePurchase_WithInvalidPrice_ThrowsValidationException(decimal price)
    {
        var content = new List<NewPurchaseContentDto>
        {
            new()
            {
                ArticleId = _article.Id,
                Count = 10,
                Price = price
            }
        };

        var command = new CreatePurchaseCommand(content, _currency.Id, "Test Comment",
            _user.Id, _transaction.Id, _storage.Name, _user.Id, DateTime.Now);

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreatePurchase_WithInvalidCount_ThrowsValidationException(int count)
    {
        var content = new List<NewPurchaseContentDto>
        {
            new()
            {
                ArticleId = _article.Id,
                Count = count,
                Price = 100.50m
            }
        };

        var command = new CreatePurchaseCommand(content, _currency.Id, "Test Comment",
            _user.Id, _transaction.Id, _storage.Name, _user.Id, DateTime.Now);

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreatePurchase_WithNonExistentStorage_ThrowsDbValidationException()
    {
        var content = new List<NewPurchaseContentDto>
        {
            new()
            {
                ArticleId = _article.Id,
                Count = 10,
                Price = 100.50m
            }
        };

        var command = new CreatePurchaseCommand(content, _currency.Id, "Test Comment",
            _user.Id, _transaction.Id, "NonExistentStorage", _user.Id, DateTime.Now);

        var ex = await Assert.ThrowsAsync<DbValidationException>(async () => await _mediator.Send(command));
        Assert.Contains(ex.Failures, f => f.ErrorName == ApplicationErrors.StoragesNotFound);
    }

    [Fact]
    public async Task CreatePurchase_WithNonExistentArticle_ThrowsDbValidationException()
    {
        var content = new List<NewPurchaseContentDto>
        {
            new()
            {
                ArticleId = 999999,
                Count = 10,
                Price = 100.50m
            }
        };

        var command = new CreatePurchaseCommand(content, _currency.Id, "Test Comment",
            _user.Id, _transaction.Id, _storage.Name, _user.Id, DateTime.Now);

        var ex = await Assert.ThrowsAsync<DbValidationException>(async () => await _mediator.Send(command));
        Assert.Contains(ex.Failures, f => f.ErrorName == ApplicationErrors.ArticlesNotFound);
    }
}
