using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MonoliteUnicorn.Configs;
using MonoliteUnicorn.EndPoints.Storages.AddContentToStorage;
using MonoliteUnicorn.PostGres.Main;
using Tests.MockData;
using Tests.testContainers.Combined;
using static Tests.MockData.MockData;

namespace Tests.HandlersTests.Storages;

[Collection("Combined collection")]
public class AddContentToStorageTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    
    public AddContentToStorageTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _mediator = sp.GetService<IMediator>()!;
        _context = sp.GetRequiredService<DContext>();
    }
        
    public async Task InitializeAsync()
    {
        await _context.AddMockProducersAndArticles();
        await _context.AddMockStorages(3);
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task AddContentToStorage_WithInvalidPrice_FailsValidation()
    {
        var content = CreateNewStorageContentDto([1], [1], 10);
        content.Last().BuyPrice = 0.001m;
        var command = new AddContentToStorageCommand(content, "Основной", "sdfsdfs");
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task AddContentToStorage_WithInvalidCount_FailsValidation()
    {
        var content = CreateNewStorageContentDto([1], [1], 10);
        content.Last().Count = 0;
        var command = new AddContentToStorageCommand(content, "Основной", "sdfsdfs");
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task AddContentToStorage_WithInvalidUserId_FailsValidation()
    {
        var content = CreateNewStorageContentDto([1], [1], 10);
        var command = new AddContentToStorageCommand(content, "Основной", "   ");
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task AddContentToStorage_WithInvalidStorageName_FailsValidation()
    {
        var content = CreateNewStorageContentDto([1], [1], 10);
        var command = new AddContentToStorageCommand(content, "  ", "sadfdsaf");
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _mediator.Send(command));
    }
    
    [Fact]
    public async Task AddContentToStorage_WithEmptyContentList_FailsValidation()
    {
        var command = new AddContentToStorageCommand([],"sdfsdf", "sadfdsaf");
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _mediator.Send(command));
    }
}