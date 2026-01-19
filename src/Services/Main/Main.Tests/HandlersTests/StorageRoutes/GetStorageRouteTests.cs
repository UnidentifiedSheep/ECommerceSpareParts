using Exceptions.Exceptions.StorageRoutes;
using Main.Application.Configs;
using Main.Application.Handlers.StorageRoutes.GetStorageRouteById;
using Main.Application.Handlers.StorageRoutes.GetStorageRouteByStorage;
using Main.Entities;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.StorageRoutes;

[Collection("Combined collection")]
public class GetStorageRouteTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private Storage _storageFrom = null!;
    private Storage _storageTo = null!;
    private Currency _currency = null!;

    public GetStorageRouteTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockStorage();
        await _mediator.AddMockStorage();
        await _context.AddMockCurrencies();

        _currency = await _context.Currencies.FirstAsync();
        _storageFrom = await _context.Storages.FirstAsync();
        _storageTo = await _context.Storages.FirstAsync(x => x.Name != _storageFrom.Name);
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task GetStorageRouteByStorage_WithExistingRoute_ReturnsRoute()
    {
        await _mediator.AddMockStorageRoute(_storageFrom.Name, _storageTo.Name, _currency.Id);

        var query = new GetStorageRouteByStorageQuery(_storageFrom.Name, _storageTo.Name);
        var result = await _mediator.Send(query);

        Assert.NotNull(result.StorageRoute);
        Assert.Equal(_storageFrom.Name, result.StorageRoute.FromStorageName);
        Assert.Equal(_storageTo.Name, result.StorageRoute.ToStorageName);
    }

    [Fact]
    public async Task GetStorageRouteByStorage_WithNonExistentStorages_ThrowsDbValidationException()
    {
        var query = new GetStorageRouteByStorageQuery("NonExistent", "NonExistent2");
        await Assert.ThrowsAsync<DbValidationException>(async () => await _mediator.Send(query));
    }

    [Fact]
    public async Task GetStorageRouteByStorage_WithExistingStoragesButNoRoute_ThrowsStorageRouteNotFound()
    {
        var query = new GetStorageRouteByStorageQuery(_storageFrom.Name, _storageTo.Name);
        await Assert.ThrowsAsync<StorageRouteNotFound>(async () => await _mediator.Send(query));
    }

    [Fact]
    public async Task GetStorageRouteById_WithExistingRoute_ReturnsRoute()
    {
        await _mediator.AddMockStorageRoute(_storageFrom.Name, _storageTo.Name, _currency.Id);
        var route = await _context.StorageRoutes.FirstAsync();

        var query = new GetStorageRouteByIdQuery(route.Id);
        var result = await _mediator.Send(query);

        Assert.NotNull(result.StorageRoute);
        Assert.Equal(route.Id, result.StorageRoute.Id);
        Assert.Equal(_storageFrom.Name, result.StorageRoute.FromStorageName);
        Assert.Equal(_storageTo.Name, result.StorageRoute.ToStorageName);
    }

    [Fact]
    public async Task GetStorageRouteById_WithNonExistentRoute_ThrowsStorageRouteNotFound()
    {
        var query = new GetStorageRouteByIdQuery(Guid.NewGuid());
        await Assert.ThrowsAsync<StorageRouteNotFound>(async () => await _mediator.Send(query));
    }
}
