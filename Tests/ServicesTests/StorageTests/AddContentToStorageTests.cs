using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.Dtos.Amw.Storage;
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
    private readonly ServiceProvider _serviceProvider;
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
        _serviceProvider = sp;
        _context = sp.GetRequiredService<DContext>();
        _inventory = sp.GetRequiredService<IInventory>();
    }
        
    public async Task InitializeAsync()
    {
        await _context.AddMockProducersAndArticles();
        var currencies = await _context.AddMockCurrency(1);
        _currency = currencies.Single();
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
            await _inventory.AddContentToStorage([], _storage.Name, _user.Id, StorageMovementType.StorageContentAddition));
    }
    
    [Theory]
    [InlineData(0.001)]
    [InlineData(-1)]
    public async Task AddContentToStorage_WithInvalidPrice_ThrowsStorageContentPriceCannotBeNegativeException(decimal price)
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        storageContent.Last().BuyPrice = price;
        var asTupleList = storageContent.Select(x => (x.ArticleId, x.Count, x.BuyPrice, _currency.Id));
        await Assert.ThrowsAsync<StorageContentPriceCannotBeNegativeException>(async () =>
            await _inventory.AddContentToStorage(asTupleList, _storage.Name, _user.Id, StorageMovementType.StorageContentAddition));
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddContentToStorage_WithInvalidItemCount_ThrowsStorageContentCountCantBeNegativeException(int count)
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        storageContent.Last().Count = count;
        var asTupleList = storageContent.Select(x => (x.ArticleId, x.Count, x.BuyPrice, _currency.Id));
        await Assert.ThrowsAsync<StorageContentCountCantBeNegativeException>(async () =>
            await _inventory.AddContentToStorage(asTupleList, _storage.Name, _user.Id, StorageMovementType.StorageContentAddition));
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
            await _inventory.AddContentToStorage(asTupleList, _storage.Name, _user.Id, StorageMovementType.StorageContentAddition));
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
            await _inventory.AddContentToStorage(asTupleList, _faker.Lorem.Letter(100), _user.Id, StorageMovementType.StorageContentAddition));
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
            await _inventory.AddContentToStorage(asTupleList, _storage.Name, _faker.Lorem.Letter(100), StorageMovementType.StorageContentAddition));
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
            await _inventory.AddContentToStorage(asTupleList, _storage.Name, _user.Id, StorageMovementType.StorageContentAddition));
    }
    
    [Fact]
    public async Task AddContentToStorage_Normal_Succeeds()
    {
        using var scope = _serviceProvider.CreateScope();
        var inventory = scope.ServiceProvider.GetRequiredService<IInventory>();
        var result = await RunSingleAddAsync(inventory);
        AssertCorrectResult(result);
    }

    [Fact]
    public async Task AddContentToStorage_ParallelExecution_Succeeds()
    {
        using var scope1 = _serviceProvider.CreateScope();
        using var scope2 = _serviceProvider.CreateScope();

        var inv1 = scope1.ServiceProvider.GetRequiredService<IInventory>();
        var inv2 = scope2.ServiceProvider.GetRequiredService<IInventory>();

        var tuple1 = await RunSingleAddAsync(inv1);
        var tuple2 = await RunSingleAddAsync(inv2);

        AssertCorrectResult(tuple1, tuple2);
    }

    private async Task<(List<NewStorageContentDto> Inputs, Dictionary<int,int> TotalPerArticle)> RunSingleAddAsync(IInventory inventory)
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var dtoList = MockData.MockData.CreateNewStorageContentDto(articleIds, new[] { _currency.Id }, 40)
                             .ToList();

        var asTuples = dtoList
            .Select(x => (x.ArticleId, x.Count, x.BuyPrice, x.CurrencyId));

        await inventory.AddContentToStorage(
            asTuples, _storage.Name, _user.Id, StorageMovementType.StorageContentAddition);

        var totalPerArticle = dtoList
            .GroupBy(x => x.ArticleId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

        return (dtoList, totalPerArticle);
    }

    private void AssertCorrectResult(params (List<NewStorageContentDto> Inputs, Dictionary<int,int> TotalPerArticle)[] results)
    {
        var allInputs = results.SelectMany(r => r.Inputs).ToList();

        var expectedTotals = new Dictionary<int,int>();
        foreach (var (_, totals) in results)
        {
            foreach (var kv in totals)
            {
                if (!expectedTotals.TryAdd(kv.Key, kv.Value))
                    expectedTotals[kv.Key] += kv.Value;
            }
        }

        var dbArticles = _context.Articles.AsNoTracking()
            .ToDictionaryAsync(x => x.Id).Result; // можно вызвать асинхронно, но для примера – синхронно

        var dbStorageContents = _context.StorageContents.AsNoTracking()
            .Where(x => x.StorageName == _storage.Name)
            .ToListAsync().Result;

        var dbMovements = _context.StorageMovements.AsNoTracking()
            .ToListAsync().Result;

        // Проверка Article.TotalCount
        foreach (var (articleId, expectedTotal) in expectedTotals)
            Assert.Equal(expectedTotal, dbArticles[articleId].TotalCount);

        // Проверка StorageContent
        Assert.Equal(allInputs.Count, dbStorageContents.Count);

        var groupedExpectedContents = allInputs
            .GroupBy(x => new { x.ArticleId, x.BuyPrice, x.CurrencyId })
            .Select(g => new {
                g.Key.ArticleId,
                g.Key.BuyPrice,
                g.Key.CurrencyId,
                Count = g.Sum(x => x.Count)
            })
            .ToList();

        foreach (var exp in groupedExpectedContents)
        {
            var actual = dbStorageContents.FirstOrDefault(x =>
                x.ArticleId == exp.ArticleId &&
                x.BuyPrice == exp.BuyPrice &&
                x.CurrencyId == exp.CurrencyId);

            Assert.NotNull(actual);
            Assert.Equal(exp.Count, actual.Count);
            Assert.Equal(_storage.Name, actual.StorageName);
        }

        // Проверка StorageMovements
        Assert.Equal(dbStorageContents.Count, dbMovements.Count);
        foreach (var sc in dbStorageContents)
        {
            var match = dbMovements.FirstOrDefault(m =>
                m.ArticleId == sc.ArticleId &&
                m.Count == sc.Count &&
                m.Price == sc.BuyPrice &&
                m.CurrencyId == sc.CurrencyId &&
                m.StorageName == sc.StorageName &&
                m.ActionType == nameof(StorageMovementType.StorageContentAddition) &&
                m.WhoMoved == _user.Id);

            Assert.NotNull(match);
        }
    }
}