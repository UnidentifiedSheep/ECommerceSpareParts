using FluentValidation;
using Main.Abstractions.Consts;
using Main.Application.Configs;
using Main.Application.Handlers.StorageRoutes.AddStorageRoute;
using Main.Entities;
using Main.Enums;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.StorageRoutes;

[Collection("Combined collection")]
public class AddStorageRouteTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private Storage _storageFrom = null!;
    private Storage _storageTo = null!;
    private Currency _currency = null!;
    
    public AddStorageRouteTests(CombinedContainerFixture fixture)
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
    public async Task AddStorageRoute_WithValidData_Succeeds()
    {
        var command = new AddStorageRouteCommand(_storageFrom.Name, _storageTo.Name, 1000,
            RouteType.IntraCity, LogisticPricingType.PerOrder, 60, 10.5m, 20.5m,
            _currency.Id, 5.0m, null);
        
        var result = await _mediator.Send(command);

        var route = await _context.StorageRoutes.FirstOrDefaultAsync(x => x.Id == result.RouteId);
        Assert.NotNull(route);
        Assert.Equal(_storageFrom.Name, route.FromStorageName);
        Assert.Equal(_storageTo.Name, route.ToStorageName);
        Assert.Equal(1000, route.DistanceM);
        Assert.Equal(10.5m, route.PriceKg);
    }

    [Fact]
    public async Task AddStorageRoute_WithInvalidDistance_ThrowsValidationException()
    {
        var command = new AddStorageRouteCommand(_storageFrom.Name, _storageTo.Name, 0, RouteType.IntraCity,
            LogisticPricingType.PerOrder, 60, 10.5m, 20.5m, _currency.Id,
            5.0m, null);

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task AddStorageRoute_WithInvalidPricePrecision_ThrowsValidationException()
    {
        var command = new AddStorageRouteCommand(_storageFrom.Name, _storageTo.Name, 1000, RouteType.IntraCity,
            LogisticPricingType.PerOrder, 60, 10.555m, 20.5m, _currency.Id, 5.0m, null);

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task AddStorageRoute_WithNonExistentCurrency_ThrowsValidationException()
    {
        var command = new AddStorageRouteCommand(_storageFrom.Name, _storageTo.Name, 1000, RouteType.IntraCity, 
            LogisticPricingType.PerOrder, 60, 10.5m, 20.5m, 9999, 5.0m, null);

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task AddStorageRoute_WithNonExistentStorage_ThrowsDbValidationException()
    {
        var command = new AddStorageRouteCommand("NonExistentStorage", _storageTo.Name, 1000, RouteType.IntraCity,
            LogisticPricingType.PerOrder, 60, 10.5m, 20.5m, _currency.Id, 5.0m, null);

        var exception = await Assert.ThrowsAsync<DbValidationException>(async () => await _mediator.Send(command));
        Assert.Contains(exception.Failures, f => f.ErrorName == ApplicationErrors.StoragesNotFound);
    }
}