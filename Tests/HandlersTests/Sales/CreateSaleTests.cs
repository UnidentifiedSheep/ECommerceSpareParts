using Application.Handlers.Sales.CreateSale;
using Core.Dtos.Amw.Sales;
using Core.Entities;
using Core.Exceptions.Balances;
using Core.Exceptions.Storages;
using Core.Models;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.Sales;

[Collection("Combined collection")]
public class CreateSaleTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    
    private Currency _currency = null!;
    private AspNetUser _user = null!;
    private Transaction _transaction = null!;
    private Storage _storage = null!;
    
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

        _user = await _context.AspNetUsers.FirstAsync();
        _storage = await _context.Storages.FirstAsync();
        var articleIds = await _context.Articles.Select(a => a.Id).ToListAsync();
        var storage = await _context.Storages.FirstAsync();
        var currency = await _context.Currencies.FirstAsync();

        await _mediator.AddMockStorageContents(articleIds, currency.Id, storage.Name, _user.Id, 10);
        
        var receiver = await _context.AspNetUsers.FirstAsync(x => x.Id != _user.Id);
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
            new NewSaleContentDto
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
            new NewSaleContentDto
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
            new NewSaleContentDto
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
            "non-existent-id", _storage.Name, DateTime.Now, null);
        await Assert.ThrowsAsync<TransactionNotFound>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task CreateSale_WithInvalidStorageName_ThrowsTransactionDoesntExistsException()
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
        await Assert.ThrowsAsync<StorageNotFoundException>(async () => await _mediator.Send(command));
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
            articlesTakenCount[content.ArticleId] = articlesTakenCount.GetValueOrDefault(content.ArticleId) + content.Count;
            
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