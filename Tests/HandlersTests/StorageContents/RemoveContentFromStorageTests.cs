using Main.Application.Configs;
using Main.Application.Handlers.StorageContents.RemoveContent;
using Core.Entities;
using Core.Enums;
using Exceptions.Exceptions.Storages;
using Exceptions.Exceptions.Users;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.StorageContents;

[Collection("Combined collection")]
public class RemoveContentFromStorageTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private List<StorageContent> _storageContents = null!;
    private List<Storage> _storages = null!;
    private User _user = null!;

    public RemoveContentFromStorageTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockProducersAndArticles();
        await _context.AddMockCurrencies();
        await _mediator.AddMockUser();
        await _mediator.AddMockStorage();
        await _mediator.AddMockStorage();

        _storages = await _context.Storages.ToListAsync();
        _user = await _context.Users.FirstAsync();
        var articleIds = await _context.Articles.Select(a => a.Id).ToListAsync();
        var currency = await _context.Currencies.FirstAsync();

        foreach (var storage in _storages)
            await _mediator.AddMockStorageContents(articleIds, currency.Id, storage.Name, _user.Id, 50);

        _storageContents = await _context.StorageContents.ToListAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task RemoveContentFromStorage_WithInvalidUserId_ThrowsUserNotFoundException()
    {
        var storageName = _storages.First().Name;
        var userId = Global.Faker.Random.Guid();
        var article = _storageContents.First();

        var content = new Dictionary<int, int>
        {
            [article.Id] = 1
        };

        var command = new RemoveContentCommand(content, userId, storageName, false, StorageMovementType.Sale);
        await Assert.ThrowsAsync<UserNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task RemoveContentFromStorage_WithEmptyContent_ThrowsArgumentException()
    {
        var command = new RemoveContentCommand([], _user.Id, _storages.First().Name, false, StorageMovementType.Sale);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task RemoveContentFromStorage_WithoutStorageNameAndNoOtherStorages_ThrowsStorageIsUnknownException()
    {
        var article = _storageContents.First();
        var content = new Dictionary<int, int>
        {
            [article.Id] = 1
        };

        var command = new RemoveContentCommand(content, _user.Id, null, false, StorageMovementType.Sale);

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task RemoveContentFromStorage_WithNegativeCount_ThrowsArgumentException(int count)
    {
        var article = _storageContents.First();

        var content = new Dictionary<int, int>
        {
            [article.Id] = count
        };

        var command =
            new RemoveContentCommand(content, _user.Id, _storages.First().Name, false, StorageMovementType.Sale);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task RemoveContentFromStorage_WithInsufficientStock_ThrowsNotEnoughCountOnStorageException()
    {
        var storageContent = _storageContents.First();
        var article = await _context.Articles.AsNoTracking().FirstAsync(x => x.Id == storageContent.ArticleId);
        var content = new Dictionary<int, int>
        {
            [article.Id] = article.TotalCount + 1000
        };

        var command = new RemoveContentCommand(content, _user.Id, storageContent.StorageName, false,
            StorageMovementType.Sale);

        await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task RemoveContentFromStorage_ValidSingleStorage_SuccessfullyRemoves()
    {
        var storageContent = _storageContents.First();
        var totalCount = _storageContents
            .Where(x => x.ArticleId == storageContent.ArticleId &&
                        x.StorageName == storageContent.StorageName)
            .Sum(x => x.Count);
        var countToRemove = Global.Faker.Random.Int(1, totalCount);

        var content = new Dictionary<int, int>
        {
            [storageContent.ArticleId] = countToRemove
        };

        var command = new RemoveContentCommand(content, _user.Id, storageContent.StorageName, false,
            StorageMovementType.Sale);


        var result = await _mediator.Send(command);

        var updatedCount = await _context.StorageContents
            .AsNoTracking()
            .Where(x => x.StorageName == storageContent.StorageName
                        && x.ArticleId == storageContent.ArticleId)
            .SumAsync(x => x.Count);

        Assert.NotEmpty(result.Changes);
        Assert.Equal(totalCount - countToRemove, updatedCount);
        Assert.Equal(countToRemove, result.Changes.Sum(r => r.Prev.Count - r.NewValue.Count));
    }

    [Fact]
    public async Task RemoveContentFromStorage_TakeFromMultipleStorages_SuccessfullyRemoves()
    {
        var articleId = _storageContents.First().ArticleId;
        var totalCount = _storageContents
            .Where(x => x.ArticleId == articleId)
            .Sum(x => x.Count);

        var content = new Dictionary<int, int>
        {
            [articleId] = totalCount
        };

        var command = new RemoveContentCommand(content, _user.Id, null, true, StorageMovementType.Sale);


        var result = await _mediator.Send(command);

        var updated = _context.StorageContents.AsNoTracking()
            .Where(x => x.ArticleId == articleId).ToList();
        Assert.Equal(0, updated.Sum(x => x.Count));
        Assert.Equal(totalCount, result.Changes.Sum(r => r.Prev.Count - r.NewValue.Count));
    }

    [Fact]
    public async Task RemoveContentFromStorage_SavesMovementWithCorrectData()
    {
        var storageContent = _storageContents.First();
        var content = new Dictionary<int, int>
        {
            [storageContent.ArticleId] = 1
        };

        var command = new RemoveContentCommand(content, _user.Id, storageContent.StorageName, false,
            StorageMovementType.Sale);
        await _mediator.Send(command);

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