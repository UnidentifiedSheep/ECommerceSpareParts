using Core.Models;
using Exceptions.Base;
using Exceptions.Exceptions.Currencies;
using Exceptions.Exceptions.Storages;
using FluentValidation;
using Main.Application.Configs;
using Main.Application.Handlers.StorageContents.EditContent;
using Main.Core.Dtos.Amw.Storage;
using Main.Core.Entities;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using User = Main.Core.Entities.User;

namespace Tests.HandlersTests.StorageContents;

[Collection("Combined collection")]
public class EditStorageContentTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private List<StorageContent> _storageContents = null!;
    private User _user = null!;

    public EditStorageContentTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockUser();
        await _mediator.AddMockProducersAndArticles();
        await _mediator.AddMockStorage();
        await _context.AddMockCurrencies();

        _user = await _context.Users.FirstAsync();
        var articleIds = await _context.Articles.Select(a => a.Id).ToListAsync();
        var storage = await _context.Storages.FirstAsync();
        var currency = await _context.Currencies.FirstAsync();

        await _mediator.AddMockStorageContents(articleIds, currency.Id, storage.Name, _user.Id, 10);
        _storageContents = await _context.StorageContents.ToListAsync();
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
        var concurrentCode = "";
        var dict = new Dictionary<int, (PatchStorageContentDto, string)>
            { [_storageContents.First().Id] = (dto, concurrentCode) };

        var command = new EditStorageContentCommand(dict, _user.Id);

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.001)]
    [InlineData(0.0001)]
    [InlineData(-1)]
    public async Task EditStorageContent_WithInvalidPrice_ThrowsStorageContentPriceCannotBeNegativeException(
        decimal price)
    {
        var dto = new PatchStorageContentDto
        {
            BuyPrice = new PatchField<decimal>
            {
                IsSet = true,
                Value = price
            }
        };
        var concurrentCode = "";
        var dict = new Dictionary<int, (PatchStorageContentDto, string)>
            { [_storageContents.First().Id] = (dto, concurrentCode) };

        var command = new EditStorageContentCommand(dict, _user.Id);

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
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
        var concurrentCode = "";
        var dict = new Dictionary<int, (PatchStorageContentDto, string)> { [999999] = (dto, concurrentCode) };

        var command = new EditStorageContentCommand(dict, _user.Id);

        await Assert.ThrowsAsync<StorageContentNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditStorageContent_ValidInput_Succeeds()
    {
        var prevMovementCount = await _context.StorageMovements.CountAsync();
        var content = await _context.StorageContents
            .AsNoTracking().FirstAsync();
        var articleBefore = await _context.Articles
            .AsNoTracking()
            .FirstAsync(x => x.Id == content.ArticleId);
        var dto = new PatchStorageContentDto
        {
            Count = new PatchField<int>
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

        var concurrentCode = "";
        var dict = new Dictionary<int, (PatchStorageContentDto, string)> { [content.Id] = (dto, concurrentCode) };

        var command = new EditStorageContentCommand(dict, _user.Id);

        var ex = await Assert.ThrowsAsync<ConcurrencyCodeMismatchException>(async () => await _mediator.Send(command));

        concurrentCode = ex.ServerCode!;
        dict = new Dictionary<int, (PatchStorageContentDto, string)> { [content.Id] = (dto, concurrentCode) };
        command = new EditStorageContentCommand(dict, _user.Id);
        await _mediator.Send(command);

        var updated = await _context.StorageContents.FindAsync(content.Id);
        var articleAfter = await _context.Articles
            .AsNoTracking()
            .FirstAsync(x => x.Id == content.ArticleId);
        var currMovementCount = await _context.StorageMovements.CountAsync();
        var movement = await _context.StorageMovements.OrderBy(x => x.Id).LastOrDefaultAsync();

        Assert.NotNull(movement);
        Assert.Equal(prevMovementCount + 1, currMovementCount);
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
        var concurrentCode = "";
        var dict = new Dictionary<int, (PatchStorageContentDto, string)>
            { [_storageContents.First().Id] = (dto, concurrentCode) };

        var command = new EditStorageContentCommand(dict, _user.Id);
        await Assert.ThrowsAsync<CurrencyNotFoundException>(async () => await _mediator.Send(command));
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
        var concurrentCode = "";
        var dict = new Dictionary<int, (PatchStorageContentDto, string)>
            { [_storageContents.First().Id] = (dto, concurrentCode) };

        var command = new EditStorageContentCommand(dict, _user.Id);
        await Assert.ThrowsAsync<StorageNotFoundException>(async () => await _mediator.Send(command));
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
        var concurrentCode = "";
        var dict = new Dictionary<int, (PatchStorageContentDto, string)> { [content.Id] = (dto, concurrentCode) };

        var command = new EditStorageContentCommand(dict, _user.Id);

        var ex = await Assert.ThrowsAsync<ConcurrencyCodeMismatchException>(async () => await _mediator.Send(command));

        concurrentCode = ex.ServerCode!;
        dict = new Dictionary<int, (PatchStorageContentDto, string)> { [content.Id] = (dto, concurrentCode) };
        command = new EditStorageContentCommand(dict, _user.Id);

        await _mediator.Send(command);

        var updated = await _context.StorageContents.FindAsync(content.Id);
        Assert.NotNull(updated);
        Assert.Equal(content.Count + 3, updated.Count);
        Assert.Equal(content.BuyPrice + 7, updated.BuyPrice);
    }
}