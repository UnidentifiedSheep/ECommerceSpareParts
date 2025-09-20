using Application.Configs;
using Application.Handlers.StorageContents.DeleteContent;
using Core.Entities;
using Exceptions.Base;
using Exceptions.Exceptions.Storages;
using Exceptions.Exceptions.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.StorageContents;

[Collection("Combined collection")]
public class DeleteContentFromStorageTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private List<StorageContent> _storageContents = null!;
    private User _user = null!;

    public DeleteContentFromStorageTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockProducersAndArticles();
        await _mediator.AddMockStorage();
        await _context.AddMockCurrencies();
        await _mediator.AddMockUser();

        _user = await _context.Users.FirstAsync();
        var currency = await _context.Currencies.FirstAsync();
        var storage = await _context.Storages.FirstAsync();
        var articleIds = await _context.Articles.Select(a => a.Id).ToListAsync();
        await _mediator.AddMockStorageContents(articleIds, currency.Id, storage.Name, _user.Id);
        _storageContents = await _context.StorageContents.ToListAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task DeleteContentFromStorage_WithInvalidUserId_ThrowsUserNotFoundException()
    {
        var contentId = _storageContents.First().Id;
        var command = new DeleteStorageContentCommand(contentId, "", Guid.Empty);
        await Assert.ThrowsAsync<UserNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task DeleteContentFromStorage_WithInvalidContentId_ThrowsStorageContentNotFoundException()
    {
        var command = new DeleteStorageContentCommand(99999, "", _user.Id);
        await Assert.ThrowsAsync<StorageContentNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task DeleteContentFromStorage_WithValidData_Succeeds()
    {
        var content = _storageContents.First();
        var prevStorageMovements = await _context.StorageMovements.CountAsync();

        var concurrencyCode = "";
        var command = new DeleteStorageContentCommand(content.Id, concurrencyCode, _user.Id);
        var ex = await Assert.ThrowsAsync<ConcurrencyCodeMismatchException>(async () => await _mediator.Send(command));

        concurrencyCode = ex.ServerCode!;
        command = new DeleteStorageContentCommand(content.Id, concurrencyCode, _user.Id);

        await _mediator.Send(command);
        var currStorageMovements = await _context.StorageMovements.CountAsync();
        Assert.Equal(prevStorageMovements + 1, currStorageMovements);
    }
}