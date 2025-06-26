using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions.Articles;
using MonoliteUnicorn.Exceptions.Currencies;
using MonoliteUnicorn.Exceptions.Storages;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Inventory;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.ServicesTests.StorageTests;

[Collection("Combined collection")]
public class AddContentToStorageTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IInventory _inventory;
    private readonly Faker _faker = new(MockData.MockData.Locale);
    
    private Currency _currency = null!;
    private Storage _storage = null!;
    private AspNetUser _user = null!;
    private List<Article> _articles = null!;
    
    public AddContentToStorageTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _inventory = sp.GetRequiredService<IInventory>();
    }
        
    public async Task InitializeAsync()
    {
        await _context.AddMockProducersAndArticles();
        _currency = await _context.AddMockCurrency();
        var storageList = await _context.AddMockStorages(1);
        _storage = storageList.First();
        _articles = await _context.Articles.ToListAsync();
        _user = await _context.AddMockUser();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task AddContentToStorage_WithEmptyContentList_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _inventory.AddContentToStorage([], _storage.Name, _user.Id, StorageContentStatus.Ok,
                StorageMovementType.StorageContentAddition));
    }
    
    [Fact]
    public async Task AddContentToStorage_WithTooPrecisePrice_ThrowsStorageContentPriceCannotBeNegativeException()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        storageContent.Last().BuyPrice = 0.001m;
        var asTupleList = storageContent.Select(x => (x.ArticleId, x.Count, x.BuyPrice, _currency.Id));
        await Assert.ThrowsAsync<StorageContentPriceCannotBeNegativeException>(async () =>
            await _inventory.AddContentToStorage(asTupleList, _storage.Name, _user.Id, StorageContentStatus.Ok,
                StorageMovementType.StorageContentAddition));
    }
    
    [Fact]
    public async Task AddContentToStorage_WithNegativePrice_ThrowsStorageContentPriceCannotBeNegativeException()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        storageContent.Last().BuyPrice = -1;
        var asTupleList = storageContent.Select(x => (x.ArticleId, x.Count, x.BuyPrice, _currency.Id));
        await Assert.ThrowsAsync<StorageContentPriceCannotBeNegativeException>(async () =>
            await _inventory.AddContentToStorage(asTupleList, _storage.Name, _user.Id, StorageContentStatus.Ok,
                StorageMovementType.StorageContentAddition));
    }
    
    [Fact]
    public async Task AddContentToStorage_WithZeroItemCount_ThrowsStorageContentCountCantBeNegativeException()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        storageContent.Last().Count = 0;
        var asTupleList = storageContent.Select(x => (x.ArticleId, x.Count, x.BuyPrice, _currency.Id));
        await Assert.ThrowsAsync<StorageContentCountCantBeNegativeException>(async () =>
            await _inventory.AddContentToStorage(asTupleList, _storage.Name, _user.Id, StorageContentStatus.Ok,
                StorageMovementType.StorageContentAddition));
    }
    
    [Fact]
    public async Task AddContentToStorage_WithNegativeItemCount_ThrowsStorageContentCountCantBeNegativeException()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        storageContent.Last().Count = -1;
        var asTupleList = storageContent.Select(x => (x.ArticleId, x.Count, x.BuyPrice, _currency.Id));
        await Assert.ThrowsAsync<StorageContentCountCantBeNegativeException>(async () =>
            await _inventory.AddContentToStorage(asTupleList, _storage.Name, _user.Id, StorageContentStatus.Ok,
                StorageMovementType.StorageContentAddition));
    }
    
    [Fact]
    public async Task AddContentToStorage_WithInvalidCurrencyId_ThrowsCurrencyNotFoundException()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        storageContent.Last().CurrencyId = int.MaxValue;
        var asTupleList = storageContent.Select(x => (x.ArticleId, x.Count, x.BuyPrice, x.CurrencyId));
        await Assert.ThrowsAsync<CurrencyNotFoundException>(async () =>
            await _inventory.AddContentToStorage(asTupleList, _storage.Name, _user.Id, StorageContentStatus.Ok,
                StorageMovementType.StorageContentAddition));
    }
    
    [Fact]
    public async Task AddContentToStorage_WithInvalidStorageName_ThrowsStorageNotFoundException()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        var asTupleList = storageContent.Select(x => (x.ArticleId, x.Count, x.BuyPrice, _currency.Id));
        await Assert.ThrowsAsync<StorageNotFoundException>(async () =>
            await _inventory.AddContentToStorage(asTupleList, _faker.Lorem.Letter(100), _user.Id, StorageContentStatus.Ok,
                StorageMovementType.StorageContentAddition));
    }
    
    [Fact]
    public async Task AddContentToStorage_WithInvalidUserId_ThrowsUserNotFoundException()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        var asTupleList = storageContent.Select(x => (x.ArticleId, x.Count, x.BuyPrice, _currency.Id));
        await Assert.ThrowsAsync<UserNotFoundException>(async () =>
            await _inventory.AddContentToStorage(asTupleList, _storage.Name, _faker.Lorem.Letter(100), StorageContentStatus.Ok,
                StorageMovementType.StorageContentAddition));
    }
    
    [Fact]
    public async Task AddContentToStorage_WithInvalidArticleId_ThrowsArticleNotFoundException()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        storageContent.Last().ArticleId = int.MaxValue;
        var asTupleList = storageContent.Select(x => (x.ArticleId, x.Count, x.BuyPrice, _currency.Id));
        await Assert.ThrowsAsync<ArticleNotFoundException>(async () =>
            await _inventory.AddContentToStorage(asTupleList, _storage.Name, _user.Id, StorageContentStatus.Ok,
                StorageMovementType.StorageContentAddition));
    }
    
    [Fact]
    public async Task AddContentToStorage_Normal_Succeeds()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 40)
            .ToList();
        var asTupleList = storageContent.Select(x => (x.ArticleId, x.Count, x.BuyPrice, _currency.Id));
        await _inventory.AddContentToStorage(asTupleList, _storage.Name, _user.Id,
            StorageContentStatus.Ok,
            StorageMovementType.StorageContentAddition);
        
        var articles = await _context.Articles.AsNoTracking()
            .ToDictionaryAsync(x => x.Id);
        //Проверка на то увеличено ли для всех артикулов общее количество
        var articlesAddedCount = storageContent.GroupBy(x => x.ArticleId)
            .ToDictionary(x => x.Key, x => x.Sum(z => z.Count));
        foreach (var (id, count) in articlesAddedCount)
            Assert.Equal(articles[id].TotalCount, count);
        
        // Проверка: содержимое склада соответствует ожидаемому
        var dbStorageContents = await _context.StorageContents
            .AsNoTracking()
            .Where(x => x.StorageName == _storage.Name)
            .ToListAsync();
        
        var expectedContents = storageContent
            .GroupBy(x => new { x.ArticleId, x.BuyPrice, x.CurrencyId }) // группируем по уникальной комбинации
            .Select(g => new
            {
                g.Key.ArticleId,
                g.Key.BuyPrice,
                g.Key.CurrencyId,
                Count = g.Sum(x => x.Count)
            })
            .ToList();
        
        foreach (var expected in expectedContents)
        {
            var actual = dbStorageContents.FirstOrDefault(x =>
                x.ArticleId == expected.ArticleId &&
                x.BuyPrice == expected.BuyPrice &&
                x.CurrencyId == expected.CurrencyId);

            Assert.NotNull(actual);
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(nameof(StorageContentStatus.Ok), actual.Status);
            Assert.Equal(_storage.Name, actual.StorageName);
        }
        
        // Проверка: правильно ли записаны движения по складу
        var storageMovements = await _context.StorageMovements.AsNoTracking().ToListAsync();

        // Ожидаемое количество движений должно соответствовать количеству записей в StorageContent
        Assert.Equal(dbStorageContents.Count, storageMovements.Count);

        // Сопоставление по каждой записи
        foreach (var sc in dbStorageContents)
        {
            var matchingMovement = storageMovements.FirstOrDefault(m =>
                m.ArticleId == sc.ArticleId &&
                m.Count == sc.Count &&
                m.Price == sc.BuyPrice &&
                m.CurrencyId == sc.CurrencyId &&
                m.StorageName == sc.StorageName);

            Assert.NotNull(matchingMovement);
            Assert.Equal(nameof(StorageMovementType.StorageContentAddition), matchingMovement.ActionType);
            Assert.Equal(_user.Id, matchingMovement.WhoMoved);
            storageMovements.Remove(matchingMovement);
        }
    }
}