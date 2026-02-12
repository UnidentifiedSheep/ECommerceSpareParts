using Main.Abstractions.Constants;
using Main.Application.Handlers.Sales.CreateSale;
using Main.Abstractions.Dtos.Amw.Sales;
using Main.Entities;
using Main.Abstractions.Models;
using Main.Persistence.Context;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using ValidationException = FluentValidation.ValidationException;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.Sales;

[Collection("Combined collection")]
public class CreateSaleTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    private Currency _currency = null!;
    private Storage _storage = null!;
    private Transaction _transaction = null!;
    private User _user = null!;

    public CreateSaleTests(CombinedContainerFixture fixture)
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
        var articleIds = await _context.Articles.Select(a => a.Id).ToListAsync();
        var storage = await _context.Storages.FirstAsync();
        var currency = await _context.Currencies.FirstAsync();

        await _mediator.AddMockStorageContents(articleIds, currency.Id, storage.Name, _user.Id, 10);

        var receiver = await _context.Users.FirstAsync(x => x.Id != _user.Id);
        await _mediator.AddMockTransaction(_user.Id, receiver.Id, _user.Id, 1212.21m);
        _transaction = await _context.Transactions.FirstAsync();
        _currency = await _context.Currencies.FirstAsync(x => x.Id == _transaction.CurrencyId);
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task CreateSale_WithEmptySaleContent_ThrowsSalesContentEmptyException()
    {
        var command = new CreateSaleCommand([], [], _currency.Id, _user.Id, _user.Id,
            _transaction.Id, _storage.Name, DateTime.Now, null);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(0.0001)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateSale_WithInvalidPrice_ThrowsSaleContentPriceOrCountException(decimal price)
    {
        var buyer = _user;
        var creator = _user;
        var currency = _currency;
        var transaction = _transaction;

        var article = await _context.Articles.FirstAsync();

        var saleContent = new List<NewSaleContentDto>
        {
            new()
            {
                ArticleId = article.Id,
                Count = 1,
                Price = price,
                PriceWithDiscount = price
            }
        };

        var command = new CreateSaleCommand(saleContent, [], currency.Id, buyer.Id, creator.Id,
            transaction.Id, _storage.Name, DateTime.Now, null);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateSale_WithInvalidCount_ThrowsSaleContentPriceOrCountException(int count)
    {
        var article = await _context.Articles.FirstAsync();

        var saleContent = new List<NewSaleContentDto>
        {
            new()
            {
                ArticleId = article.Id,
                Count = count,
                Price = 10,
                PriceWithDiscount = 10
            }
        };

        var command = new CreateSaleCommand(saleContent, [], _currency.Id, _user.Id, _user.Id,
            _transaction.Id, _storage.Name, DateTime.Now, null);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateSale_WithNotEnoughDetails_ThrowsArgumentException()
    {
        var article = await _context.Articles.FirstAsync();

        var saleContent = new List<NewSaleContentDto>
        {
            new()
            {
                ArticleId = article.Id,
                Count = 100000,
                Price = 10,
                PriceWithDiscount = 9.5m
            }
        };

        var command = new CreateSaleCommand(saleContent, [], _currency.Id, _user.Id, _user.Id,
            _transaction.Id, _storage.Name, DateTime.Now, null);
        await Assert.ThrowsAsync<ArgumentException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task CreateSale_WithInvalidTransaction_ThrowsTransactionNotFoundException()
    {
        var storageContent = await _context.StorageContents.FirstAsync();
        var article = await _context.Articles.FirstAsync(x => x.Id == storageContent.ArticleId);
        var saleContent = new List<NewSaleContentDto>
        {
            new()
            {
                ArticleId = article.Id,
                Count = storageContent.Count,
                Price = 10,
                PriceWithDiscount = 9
            }
        };

        var newValue = storageContent.Adapt<StorageContent>();
        newValue.Count = 0;
        var storageContentValues = new List<PrevAndNewValue<StorageContent>>
        {
            new(storageContent, newValue)
        };

        var command = new CreateSaleCommand(saleContent, storageContentValues, _currency.Id, _user.Id, _user.Id,
            Guid.Empty, _storage.Name, DateTime.Now, null);
        var exception = await Assert.ThrowsAsync<DbValidationException>(async () => await _mediator.Send(command));
        Assert.Equal(ApplicationErrors.TransactionsNotFound, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task CreateSale_WithInvalidStorageName_ThrowsStorageNotFoundException()
    {
        var storageContent = await _context.StorageContents.FirstAsync();
        var saleContent = new List<NewSaleContentDto>
        {
            new()
            {
                ArticleId = storageContent.ArticleId,
                Count = storageContent.Count,
                Price = 10,
                PriceWithDiscount = 9
            }
        };

        var newValue = storageContent.Adapt<StorageContent>();
        newValue.Count = 0;
        var storageContentValues = new List<PrevAndNewValue<StorageContent>>
        {
            new(storageContent, newValue)
        };

        var command = new CreateSaleCommand(saleContent, storageContentValues, _currency.Id, _user.Id, _user.Id,
            _transaction.Id, "non-existing-storage-name", DateTime.Now, null);
        var exception = await Assert.ThrowsAsync<DbValidationException>(async () => await _mediator.Send(command));
        Assert.Equal(ApplicationErrors.StoragesNotFound, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task CreateSale_WithValidData_Succeeds()
    {
        var storageContent = await _context.StorageContents.ToListAsync();
        var saleContent = new List<NewSaleContentDto>();
        var storageContentValues = new List<PrevAndNewValue<StorageContent>>();
        var articlesTakenCount = new Dictionary<int, int>();

        foreach (var content in storageContent)
        {
            var newValue = content.Adapt<StorageContent>();
            newValue.Count = 0;
            saleContent.Add(new NewSaleContentDto
            {
                ArticleId = content.ArticleId,
                Count = content.Count,
                Comment = Global.Faker.Lorem.Letter(10),
                Price = content.BuyPrice,
                PriceWithDiscount = content.BuyPrice
            });
            articlesTakenCount[content.ArticleId] =
                articlesTakenCount.GetValueOrDefault(content.ArticleId) + content.Count;

            storageContentValues.Add(new PrevAndNewValue<StorageContent>(content.Adapt<StorageContent>(), newValue));
        }

        var command = new CreateSaleCommand(saleContent, storageContentValues, _currency.Id, _user.Id, _user.Id,
            _transaction.Id, _storage.Name, DateTime.Now, null);
        await _mediator.Send(command);


        var sale = await _context.Sales
            .Include(x => x.SaleContents)
            .ThenInclude(x => x.SaleContentDetails)
            .FirstOrDefaultAsync(x => x.TransactionId == _transaction.Id);


        Assert.NotNull(sale);
        foreach (var content in sale.SaleContents)
        {
            var details = content.SaleContentDetails;
            var countInDetail = details.Sum(detail => detail.Count);
            Assert.Equal(content.Count, countInDetail);
            articlesTakenCount[content.ArticleId] -= countInDetail;
        }

        Assert.All(articlesTakenCount, kvp => Assert.Equal(0, kvp.Value));
    }
}