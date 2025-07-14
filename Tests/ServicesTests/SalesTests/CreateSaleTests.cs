using Bogus;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.Dtos.Amw.Sales;
using MonoliteUnicorn.Exceptions.Balances;
using MonoliteUnicorn.Exceptions.Sales;
using MonoliteUnicorn.Exceptions.Storages;
using MonoliteUnicorn.Models;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Balances;
using MonoliteUnicorn.Services.Sale;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.ServicesTests.SalesTests;

[Collection("Combined collection")]
public class CreateSaleTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly DContext _context;
    private readonly Faker _faker = new(MockData.MockData.Locale);
    private readonly ISale _sale;
    
    private Currency _currency = null!;
    private AspNetUser _user = null!;
    private Transaction _transaction = null!;
    private Storage _storage = null!;
    
    public CreateSaleTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _serviceProvider = sp;
        _context = sp.GetRequiredService<DContext>();
        _sale = sp.GetRequiredService<ISale>();
    }
        
    public async Task InitializeAsync()
    {
        var currencies = await _context.AddMockCurrency(1);
        await _context.AddMockProducersAndArticles();
        _currency = currencies.Single();
        _user = await _context.AddMockUser();
        _storage = await _context.AddMockStorage();
        var receiver = await _context.AddMockUser();
        _transaction = (await _context.AddMockTransaction([receiver.Id], [_user.Id], _user.Id,
            [_currency.Id], 1)).Single();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }
    
    [Fact]
    public async Task CreateSale_WithEmptySaleContent_ThrowsSalesContentEmptyException()
    {
        var saleDateTime = DateTime.Now;
        
        await Assert.ThrowsAsync<SalesContentEmptyException>(async () =>
            await _sale.CreateSale([], [], _currency.Id, _user.Id, _user.Id,
                _transaction.Id, _storage.Name, saleDateTime, null));
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
            new NewSaleContentDto
            {
                ArticleId = article.Id,
                Count = 1,
                Price = price,
                PriceWithDiscount = price
            }
        };
        
        await Assert.ThrowsAsync<SaleContentPriceOrCountException>(async () =>
            await _sale.CreateSale(saleContent, new List<PrevAndNewValue<StorageContent>>(),
                currency.Id, buyer.Id, creator.Id, transaction.Id, _storage.Name, DateTime.Now, null));
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
        
        await Assert.ThrowsAsync<SaleContentPriceOrCountException>(async () =>
            await _sale.CreateSale(saleContent, new List<PrevAndNewValue<StorageContent>>(),
                _currency.Id, _user.Id, _user.Id, _transaction.Id, _storage.Name, DateTime.Now, null));
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
        
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _sale.CreateSale(saleContent, [], _currency.Id, _user.Id,
                _user.Id, _transaction.Id, _storage.Name, DateTime.Now, null));
    }
    
    [Fact]
    public async Task CreateSale_WithInvalidTransaction_ThrowsTransactionNotFoundException()
    {
        var storageContent = (await _context.AddMockStorageContent(1)).Single();
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
        
        await Assert.ThrowsAsync<TransactionNotFound>(async () =>
            await _sale.CreateSale(saleContent, storageContentValues, _currency.Id,
                _user.Id, _user.Id, "non-existent-id", _storage.Name, DateTime.Now, null));
    }
    
    [Fact]
    public async Task CreateSale_WithInvalidStorageName_ThrowsTransactionDoesntExistsException()
    {
        var storageContent = (await _context.AddMockStorageContent(1)).Single();
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
        
        await Assert.ThrowsAsync<StorageNotFoundException>(async () =>
            await _sale.CreateSale(saleContent, storageContentValues, _currency.Id,
                _user.Id, _user.Id, _transaction.Id, "non-existing-storage-name", DateTime.Now, null));
    }
    
    [Fact]
    public async Task CreateSale_WithValidData_Succeeds()
    {
        var storageContent = (await _context.AddMockStorageContent(10)).ToList();
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
                Comment = _faker.Lorem.Letter(10),
                Price = content.BuyPrice,
                PriceWithDiscount = content.BuyPrice
            });
            articlesTakenCount[content.ArticleId] = articlesTakenCount.GetValueOrDefault(content.ArticleId) + content.Count;
            
            storageContentValues.Add(new PrevAndNewValue<StorageContent>(content.Adapt<StorageContent>(), newValue));
        }

        await _sale.CreateSale(saleContent, storageContentValues, _currency.Id,
            _user.Id, _user.Id, _transaction.Id, _storage.Name, DateTime.Now, null);
        
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