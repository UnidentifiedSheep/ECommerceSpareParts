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
public class DeleteContentFromStorageTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IInventory _inventory;
    private readonly Faker _faker = new();
    private AspNetUser _user = null!;
    private List<StorageContent> _storageContents = null!;
    
    public DeleteContentFromStorageTests(CombinedContainerFixture fixture)
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
        await _context.AddMockStorages(1);
        _storageContents = (await _context.AddMockStorageContent(10)).ToList();
        _user = await _context.AddMockUser();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task DeleteContentFromStorage_WithInvalidUserId_ThrowsUserNotFoundException()
    {
        var contentId = _storageContents.First().Id;
        await Assert.ThrowsAsync<UserNotFoundException>(
            async () => await _inventory.DeleteContentFromStorage(contentId, _faker.Random.Guid().ToString(), StorageMovementType.StorageContentDeletion));
    }
    
    [Fact]
    public async Task DeleteContentFromStorage_WithInvalidContentId_ThrowsStorageContentNotFoundException()
    {
        await Assert.ThrowsAsync<StorageContentNotFoundException>(
            async () => await _inventory.DeleteContentFromStorage(_faker.Random.Int(99999), _user.Id, StorageMovementType.StorageContentDeletion));
    }
    
    [Fact]
    public async Task DeleteContentFromStorage_WithValidData_Succeeds()
    {
        var content = _storageContents.First();
        await _inventory.DeleteContentFromStorage(content.Id, _user.Id, StorageMovementType.StorageContentDeletion);
        var storageMovements = await _context.StorageMovements.ToListAsync();
        Assert.Single(storageMovements);
    }
}