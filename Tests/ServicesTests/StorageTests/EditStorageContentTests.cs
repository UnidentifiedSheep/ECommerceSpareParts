using Core.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.Dtos.Amw.Storage;
using MonoliteUnicorn.Exceptions.Currencies;
using MonoliteUnicorn.Exceptions.Storages;
using MonoliteUnicorn.PostGres.Main;
using MonoliteUnicorn.Services.Inventory;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.ServicesTests.StorageTests;

[Collection("Combined collection")]
public class EditStorageContentTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IInventory _inventory;
    private AspNetUser _user = null!;
    private List<StorageContent> _storageContents = null!;
    
    public EditStorageContentTests(CombinedContainerFixture fixture)
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
    public async Task EditStorageContent_WithNegativeCount_ThrowsException()
    {
        var dto = new PatchStorageContentDto
        {
            Count = new PatchField<int>
            {
                IsSet = true,
                Value = -1
            }
        };
        var dict = new Dictionary<int, PatchStorageContentDto> { [_storageContents.First().Id] = dto };

        await Assert.ThrowsAsync<StorageContentCountCantBeNegativeException>(async () =>
            await _inventory.EditStorageContent(dict, _user.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.001)]
    [InlineData(0.0001)]
    [InlineData(-1)]
    public async Task EditStorageContent_WithInvalidPrice_ThrowsStorageContentPriceCannotBeNegativeException(decimal price)
    {
        var dto = new PatchStorageContentDto
        {
            BuyPrice = new PatchField<decimal>
            {
                IsSet = true,
                Value = price
            }
        };
        var dict = new Dictionary<int, PatchStorageContentDto> { [_storageContents.First().Id] = dto };

        await Assert.ThrowsAsync<StorageContentPriceCannotBeNegativeException>(async () =>
            await _inventory.EditStorageContent(dict, _user.Id));
    }

    [Fact]
    public async Task EditStorageContent_WithInvalidStorageContentId_ThrowsStorageContentNotFoundException()
    {
        var dto = new PatchStorageContentDto
        {
            Count = new PatchField<int>
            {
                IsSet = true,
                Value = 6
            }
        };
        var dict = new Dictionary<int, PatchStorageContentDto> { [999999] = dto };

        await Assert.ThrowsAsync<StorageContentNotFoundException>(async () =>
            await _inventory.EditStorageContent(dict, _user.Id));
    }

    [Fact]
    public async Task EditStorageContent_ValidInput_Succeeds()
    {
        var content = await _context.StorageContents
            .AsNoTracking().FirstAsync();
        var articleBefore = await _context.Articles
            .AsNoTracking()
            .FirstAsync(x => x.Id == content.ArticleId);
        var dto = new PatchStorageContentDto { Count = new PatchField<int>
            {
                IsSet = true,
                Value = content.Count + 5
            }, 
            BuyPrice = new PatchField<decimal>
            {
                IsSet = true,
                Value = content.BuyPrice + 10
            }
        };
        var dict = new Dictionary<int, PatchStorageContentDto> { [content.Id] = dto };

        await _inventory.EditStorageContent(dict, _user.Id);

        var updated = await _context.StorageContents.FindAsync(content.Id);
        var articleAfter = await _context.Articles
            .AsNoTracking()
            .FirstAsync(x => x.Id == content.ArticleId);
        var movement = await _context.StorageMovements.FirstOrDefaultAsync();
        
        Assert.NotNull(movement);
        Assert.Equal(5, movement.Count);
        Assert.Equal(content.BuyPrice, movement.Price);
        
        Assert.Equal(articleBefore.TotalCount, articleAfter.TotalCount - 5);
        Assert.NotNull(updated);
        Assert.Equal(content.Count + 5, updated.Count);
        Assert.Equal(content.BuyPrice + 10, updated.BuyPrice);
    }
    
    [Fact]
    public async Task EditStorageContent_WithInvalidCurrencyId_ThrowsCurrencyNotFoundException()
    {
        var dto = new PatchStorageContentDto
        {
            CurrencyId = new PatchField<int>
            {
                IsSet = true,
                Value = 99999
            }
        };
        var dict = new Dictionary<int, PatchStorageContentDto> { [_storageContents.First().Id] = dto };

        await Assert.ThrowsAsync<CurrencyNotFoundException>(async () =>
            await _inventory.EditStorageContent(dict, _user.Id));
    }

    [Fact]
    public async Task EditStorageContent_WithInvalidStorageName_ThrowsStorageNotFoundException()
    {
        var dto = new PatchStorageContentDto
        {
            StorageName = new PatchField<string>
            {
                IsSet = true,
                Value = "non_existing_storage"
            }
        };
        var dict = new Dictionary<int, PatchStorageContentDto> { [_storageContents.First().Id] = dto };

        await Assert.ThrowsAsync<StorageNotFoundException>(async () =>
            await _inventory.EditStorageContent(dict, _user.Id));
    }

    [Fact]
    public async Task EditStorageContent_WithMultipleFieldsUpdate_Succeeds()
    {
        var content = await _context.StorageContents
            .AsNoTracking().FirstAsync();
        var dto = new PatchStorageContentDto
        {
            Count = new PatchField<int>
            {
                IsSet = true,
                Value = content.Count + 3
            },
            BuyPrice = new PatchField<decimal>
            {
                IsSet = true,
                Value = content.BuyPrice + 7
            },
            CurrencyId = new PatchField<int>
            {
                IsSet = true,
                Value = 1
            },
            StorageName = new PatchField<string>
            {
                IsSet = true,
                Value = content.StorageName
            }
        };
        var dict = new Dictionary<int, PatchStorageContentDto> { [content.Id] = dto };

        await _inventory.EditStorageContent(dict, _user.Id);

        var updated = await _context.StorageContents.FindAsync(content.Id);
        Assert.NotNull(updated);
        Assert.Equal(content.Count + 3, updated.Count);
        Assert.Equal(content.BuyPrice + 7, updated.BuyPrice);
    }

}