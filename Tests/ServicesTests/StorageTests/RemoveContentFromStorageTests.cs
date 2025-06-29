using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.Enums;
using MonoliteUnicorn.Exceptions.Storages;
using MonoliteUnicorn.Exceptions.Users;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Inventory;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.ServicesTests.StorageTests;

[Collection("Combined collection")]
public class RemoveContentFromStorageTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IInventory _inventory;
    private readonly Faker _faker = new();
    private AspNetUser _user = null!;
    private List<StorageContent> _storageContents = null!;
    private List<Storage> _storages = null!;
    
    public RemoveContentFromStorageTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _inventory = sp.GetRequiredService<IInventory>();
    }
        
    public async Task InitializeAsync()
    {
        await _context.AddMockProducersAndArticles();
        await _context.AddMockCurrency(1);
        _storages = (await _context.AddMockStorages(5)).ToList();
        _storageContents = (await _context.AddMockStorageContent(100)).ToList();
        _user = await _context.AddMockUser();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task RemoveContentFromStorage_WithInvalidUserId_ThrowsUserNotFoundException()
    {
        var storageName = _storages.First().Name;
        var userId = _faker.Random.Guid().ToString();
        var article = _storageContents.First();

        var content = new[] { (article.ArticleId, 1) };

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            _inventory.RemoveContentFromStorage(content, userId, storageName, false, StorageMovementType.Sale));
    }

    [Fact]
    public async Task RemoveContentFromStorage_WithEmptyContent_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _inventory.RemoveContentFromStorage([], _user.Id, "any", false, StorageMovementType.Sale));
    }

    [Fact]
    public async Task RemoveContentFromStorage_WithoutStorageNameAndNoOtherStorages_ThrowsStorageIsUnknownException()
    {
        var article = _storageContents.First();
        var content = new[] { (article.ArticleId, 1) };

        await Assert.ThrowsAsync<StorageIsUnknownException>(() =>
            _inventory.RemoveContentFromStorage(content, _user.Id, null, false, StorageMovementType.Sale));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task RemoveContentFromStorage_WithNegativeCount_ThrowsArgumentException(int count)
    {
        var article = _storageContents.First();
        var content = new[] { (article.ArticleId, count) };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _inventory.RemoveContentFromStorage(content, _user.Id, _storages.First().Name, false, StorageMovementType.Sale));
    }

    [Fact]
    public async Task RemoveContentFromStorage_WithInsufficientStock_ThrowsNotEnoughCountOnStorageException()
    {
        var storageContent = _storageContents.First();
        var article = await _context.Articles.FirstAsync(x => x.Id == storageContent.ArticleId);
        var content = new[] { (article.Id, article.TotalCount + 1000) };
        
        await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() =>
            _inventory.RemoveContentFromStorage(content, _user.Id, storageContent.StorageName, false, StorageMovementType.Sale));
    }

    [Fact]
    public async Task RemoveContentFromStorage_ValidSingleStorage_SuccessfullyRemoves()
    {
        var storageContent = _storageContents.First();
        var totalCount = _storageContents
            .Where(x => x.ArticleId == storageContent.ArticleId && 
                        x.StorageName == storageContent.StorageName)
            .Sum(x => x.Count);
        var countToRemove = _faker.Random.Int(1, totalCount);
        var content = new[] { (storageContent.ArticleId, countToRemove) };

        var result = 
            (await _inventory.RemoveContentFromStorage(content, _user.Id, storageContent.StorageName, false, StorageMovementType.Sale))
            .ToList();

        var updatedCount = await _context.StorageContents
            .AsNoTracking()
            .Where(x => x.StorageName == storageContent.StorageName 
                        && x.ArticleId == storageContent.ArticleId)
            .SumAsync(x => x.Count);
        
        Assert.NotEmpty(result);
        Assert.Equal(totalCount - countToRemove, updatedCount);
        Assert.Equal(countToRemove, result.Sum(r => r.Prev.Count - r.NewValue.Count));
    }

    [Fact]
    public async Task RemoveContentFromStorage_TakeFromMultipleStorages_SuccessfullyRemoves()
    {
        var articleId = _storageContents.First().ArticleId;
        var totalCount = _storageContents
            .Where(x => x.ArticleId == articleId)
            .Sum(x => x.Count);
        var content = new[] { (articleId, totalCount) };

        var result = await _inventory.RemoveContentFromStorage(content, _user.Id, null, 
            true, StorageMovementType.Sale);

        var updated = _context.StorageContents.AsNoTracking()
            .Where(x => x.ArticleId == articleId).ToList();
        Assert.Equal(0, updated.Sum(x => x.Count));
        Assert.Equal(totalCount, result.Sum(r => r.Prev.Count - r.NewValue.Count));
    }

    [Fact]
    public async Task RemoveContentFromStorage_MergesDuplicateArticleIds()
    {
        var storageContent = _storageContents.First();
        var content = new[] { (storageContent.ArticleId, 1), (storageContent.ArticleId, 2) };
        var countBeforeUpdate = _storageContents
            .Where(x => x.ArticleId == storageContent.ArticleId &&
                        x.StorageName == storageContent.StorageName)
            .Sum(x => x.Count);
        
        var result = await _inventory.RemoveContentFromStorage(content, _user.Id, storageContent.StorageName, 
            false, StorageMovementType.Sale);

        var updatedCount = await _context.StorageContents
            .AsNoTracking()
            .Where(x => x.StorageName == storageContent.StorageName 
                        && x.ArticleId == storageContent.ArticleId)
            .SumAsync(x => x.Count);
        
        Assert.Equal(countBeforeUpdate - 3, updatedCount);
        Assert.Equal(3, result.Sum(r => r.Prev.Count - r.NewValue.Count));
    }

    [Fact]
    public async Task RemoveContentFromStorage_SavesMovementWithCorrectData()
    {
        var storageContent = _storageContents.First();
        var content = new[] { (storageContent.ArticleId, 1) };

        await _inventory.RemoveContentFromStorage(content, _user.Id, storageContent.StorageName, false, StorageMovementType.Sale);

        var movement = await _context.StorageMovements
            .Where(m => m.ArticleId == storageContent.ArticleId && m.StorageName == storageContent.StorageName)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();

        Assert.NotNull(movement);
        Assert.Equal(nameof(StorageMovementType.Sale), movement.ActionType);
        Assert.Equal(-1, movement.Count);
        Assert.Equal(_user.Id, movement.WhoMoved);
    }
}