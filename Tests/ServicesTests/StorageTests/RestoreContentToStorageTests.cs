using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
public class RestoreContentToStorageTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly DContext _context;
    private readonly IInventory _inventory;
    private readonly Faker _faker = new(MockData.MockData.Locale);

    private List<Currency> _currency = null!;
    private AspNetUser _user = null!;
    private List<Storage> _storages = null!;
    private List<Article> _articles = null!;
    private Dictionary<int, StorageContent> _storageContent = null!;

    private const int ContentGenerationCount = 10;
    private const int InvalidStorageNameLength = 100;

    public RestoreContentToStorageTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _serviceProvider = sp;
        _context = sp.GetRequiredService<DContext>();
        _inventory = sp.GetRequiredService<IInventory>();
    }

    public async Task InitializeAsync()
    {
        await _context.AddMockProducersAndArticles();
        _currency = (await _context.AddMockCurrency(5)).ToList();
        _storages = (await _context.AddMockStorages(10)).ToList();
        _articles = await _context.Articles.ToListAsync();
        _user = await _context.AddMockUser();
        _storageContent = (await _context.AddMockStorageContent(40))
            .ToDictionary(x => x.Id);
        foreach (var content in _storageContent)
            _articles.First(x => x.Id == content.Value.ArticleId)
                .TotalCount += content.Value.Count;
        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    private List<(SaleContentDetail, int)> GenerateValidStorageContent()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storages = _storages.Select(x => x.Name).ToArray();
        var currencies = _currency.Select(x => x.Id).ToArray();

        var storageContent = MockData.MockData
            .CreateSaleContentDetails(_storageContent.Keys, storages, currencies, ContentGenerationCount)
            .Select(x => (x, x.StorageContentId == null ? _faker.PickRandom(articleIds) : _storageContent[x.StorageContentId.Value].ArticleId))
            .ToList();
        foreach (var (detail, _) in storageContent.Where(x => x.x.StorageContentId != null))
            detail.Storage = _storageContent[detail.StorageContentId!.Value].StorageName;
        return storageContent;
    }

    [Fact]
    public async Task RestoreContentToStorage_WithEmptyContentList_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _inventory.RestoreContentToStorage([], StorageMovementType.StorageContentAddition, _user.Id));
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(-1)]
    public async Task RestoreContentToStorage_WithInvalidPrice_ThrowsStorageContentPriceCannotBeNegativeException(decimal invalidPrice)
    {
        var content = GenerateValidStorageContent();
        content[^1].Item1.BuyPrice = invalidPrice;

        await Assert.ThrowsAsync<StorageContentPriceCannotBeNegativeException>(() =>
            _inventory.RestoreContentToStorage(content, StorageMovementType.StorageContentAddition, _user.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RestoreContentToStorage_WithInvalidCount_ThrowsStorageContentCountCantBeNegativeException(int count)
    {
        var content = GenerateValidStorageContent();
        content[^1].Item1.Count = count;

        await Assert.ThrowsAsync<StorageContentCountCantBeNegativeException>(() =>
            _inventory.RestoreContentToStorage(content, StorageMovementType.StorageContentAddition, _user.Id));
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public async Task RestoreContentToStorage_WithInvalidCurrencyId_ThrowsCurrencyNotFoundException(int currencyId)
    {
        var content = GenerateValidStorageContent();
        content[^1].Item1.CurrencyId = currencyId;

        await Assert.ThrowsAsync<CurrencyNotFoundException>(() =>
            _inventory.RestoreContentToStorage(content, StorageMovementType.StorageContentAddition, _user.Id));
    }

    [Fact]
    public async Task RestoreContentToStorage_WithInvalidStorageName_ThrowsStorageNotFoundException()
    {
        var content = GenerateValidStorageContent();
        content[^1].Item1.Storage = _faker.Lorem.Letter(InvalidStorageNameLength);

        await Assert.ThrowsAsync<StorageNotFoundException>(() =>
            _inventory.RestoreContentToStorage(content, StorageMovementType.StorageContentAddition, _user.Id));
    }

    [Fact]
    public async Task RestoreContentToStorage_WithInvalidUserId_ThrowsUserNotFoundException()
    {
        var content = GenerateValidStorageContent();
        var invalidUserId = _faker.Random.Guid().ToString();

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            _inventory.RestoreContentToStorage(content, StorageMovementType.StorageContentAddition, invalidUserId));
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public async Task RestoreContentToStorage_WithInvalidArticleId_ThrowsArticleNotFoundException(int invalidArticleId)
    {
        var content = GenerateValidStorageContent();
        content[^1] = (content[^1].Item1, invalidArticleId);

        await Assert.ThrowsAsync<ArticleNotFoundException>(() =>
            _inventory.RestoreContentToStorage(content, StorageMovementType.StorageContentAddition, _user.Id));
    }

    [Fact]
    public async Task RestoreContentToStorage_WithValidData_Succeeds()
    {
        var content = GenerateValidStorageContent();
        var nullIdsCount = content.Count(x => x.Item1.StorageContentId == null);
        
        await _inventory.RestoreContentToStorage(content, StorageMovementType.StorageContentAddition, _user.Id);

        var storageContents = await _context.StorageContents
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id);
        
        var articles = await _context.Articles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id);
        
        Assert.Equal(_storageContent.Count + nullIdsCount, storageContents.Count);
        foreach (var (saleContent, articleId) in content)
        {
            var dbStorageContent = saleContent.StorageContentId == null 
                ? storageContents.FirstOrDefault(x => x.Value.ArticleId == articleId &&
                                                      x.Value.Count == saleContent.Count &&
                                                      x.Value.StorageName == saleContent.Storage).Value 
                : storageContents[saleContent.StorageContentId.Value];
            
            Assert.NotNull(dbStorageContent);
            Assert.Equal(articleId, dbStorageContent.ArticleId);
            
            if(saleContent.StorageContentId == null)
                Assert.Equal(saleContent.BuyPrice, dbStorageContent.BuyPrice);
        }

        foreach (var i in storageContents)
            articles[i.Value.ArticleId].TotalCount -= i.Value.Count;
        
        Assert.All(articles, x => Assert.Equal(0, x.Value.TotalCount));
    }
    
    [Fact]
    public async Task RestoreContentToStorage_ParallelExecution_Succeeds()
    {
        using var scope = _serviceProvider.CreateScope();
        var tempInventory = scope.ServiceProvider.GetRequiredService<IInventory>();
        var content1 = GenerateValidStorageContent();
        var content2 = GenerateValidStorageContent();

        var firstNullContentsCound = content1.Count(x => x.Item1.StorageContentId == null);
        var secondNullContentsCound = content2.Count(x => x.Item1.StorageContentId == null);
        
        var task1 = _inventory.RestoreContentToStorage(content1, StorageMovementType.StorageContentAddition, _user.Id);
        var task2 = tempInventory.RestoreContentToStorage(content2, StorageMovementType.StorageContentAddition, _user.Id);

        await Task.WhenAll(task1, task2);

        var storageContents = await _context.StorageContents.AsNoTracking().ToListAsync();
        var articles = await _context.Articles.AsNoTracking().ToDictionaryAsync(x => x.Id);

        var expectedCounts = storageContents
            .GroupBy(x => x.ArticleId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));
        
        var totalRowsMustBe = _storageContent.Count + firstNullContentsCound + secondNullContentsCound;
        Assert.Equal(totalRowsMustBe, storageContents.Count);
        
        foreach (var (articleId, expectedCount) in expectedCounts)
            Assert.Equal(expectedCount, articles[articleId].TotalCount);
        
    }
}