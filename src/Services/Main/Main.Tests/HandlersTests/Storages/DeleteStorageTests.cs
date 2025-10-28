using Exceptions.Exceptions.Storages;
using Main.Application.Handlers.Storages.DeleteStorage;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.Storages;

[Collection("Combined collection")]
public class DeleteStorageTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    public DeleteStorageTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetRequiredService<IMediator>();
        _context = sp.GetRequiredService<DContext>();
    }

    public async Task InitializeAsync()
    {
        // nothing here by default
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task DeleteStorage_EmptyName_ThrowsValidationException()
    {
        var command = new DeleteStorageCommand("");
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task DeleteStorage_NotExisting_ThrowsStorageNotFoundException()
    {
        var command = new DeleteStorageCommand("some-non-existing-storage");
        await Assert.ThrowsAsync<StorageNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task DeleteStorage_WithNoContents_Succeeds()
    {
        // prepare producers/articles required by AddMockStorage
        await _mediator.AddMockProducersAndArticles();
        await _mediator.AddMockStorage();

        var storage = await _context.Storages.FirstAsync();
        var command = new DeleteStorageCommand(storage.Name);
        await _mediator.Send(command);

        var exists = await _context.Storages.AnyAsync(x => x.Name == storage.Name);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteStorage_NameWithSpaces_TrimsAndDeletes()
    {
        await _mediator.AddMockProducersAndArticles();
        await _mediator.AddMockStorage();
        var storage = await _context.Storages.FirstAsync();

        var command = new DeleteStorageCommand("  " + storage.Name + "  ");
        await _mediator.Send(command);

        var exists = await _context.Storages.AnyAsync(x => x.Name == storage.Name);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteStorage_WithContents_ThrowsDbUpdateException()
    {
        // prepare and add contents to storage
        await _mediator.AddMockProducersAndArticles();
        await _mediator.AddMockStorage();
        await _context.AddMockCurrencies();
        var userId = await _mediator.AddMockUser();

        var storage = await _context.Storages.FirstAsync();
        var currency = await _context.Currencies.FirstAsync();
        var articleIds = await _context.Articles.Select(a => a.Id).ToListAsync();

        await _mediator.AddMockStorageContents(articleIds, currency.Id, storage.Name, userId, 5);

        var command = new DeleteStorageCommand(storage.Name);
        await Assert.ThrowsAsync<DbUpdateException>(async () => await _mediator.Send(command));

        // ensure storage still exists after failed delete
        var exists = await _context.Storages.AnyAsync(x => x.Name == storage.Name);
        Assert.True(exists);
    }
}

