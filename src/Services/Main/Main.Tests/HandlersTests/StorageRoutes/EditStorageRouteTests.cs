using Core.Models;
using FluentValidation;
using Main.Application.Configs;
using Main.Application.Handlers.StorageRoutes.EditStorageRoute;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Entities;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using Exceptions.Exceptions.StorageRoutes;

namespace Tests.HandlersTests.StorageRoutes;

[Collection("Combined collection")]
public class EditStorageRouteTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private Storage _storageFrom = null!;
    private Storage _storageTo = null!;
    private Currency _currency = null!;
    
    public EditStorageRouteTests(CombinedContainerFixture fixture)
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
    public async Task EditStorageRoute_WithValidData_Succeeds()
    {
        await _mediator.AddMockStorageRoute(_storageFrom.Name, _storageTo.Name, _currency.Id);
        var route = await _context.StorageRoutes.AsNoTracking().FirstAsync();
        
        var patchDto = new PatchStorageRouteDto
        {
            DistanceM = PatchField<int>.From(2000),
            PriceKg = PatchField<decimal>.From(15.5m),
            IsActive = PatchField<bool>.From(!route.IsActive)
        };
        
        var command = new EditStorageRouteCommand(route.Id, patchDto);
        
        await _mediator.Send(command);

        var updatedRoute = await _context.StorageRoutes.AsNoTracking().FirstAsync(x => x.Id == route.Id);
        Assert.Equal(2000, updatedRoute.DistanceM);
        Assert.Equal(15.5m, updatedRoute.PriceKg);
        Assert.Equal(!route.IsActive, updatedRoute.IsActive);
        
        Assert.Equal(route.RouteType, updatedRoute.RouteType);
        Assert.Equal(route.PricingModel, updatedRoute.PricingModel);
        Assert.Equal(route.DeliveryTimeMinutes, updatedRoute.DeliveryTimeMinutes);
        Assert.Equal(route.CurrencyId, updatedRoute.CurrencyId);
    }

    [Fact]
    public async Task EditStorageRoute_WithNonExistentId_ThrowsStorageRouteNotFound()
    {
        var patchDto = new PatchStorageRouteDto
        {
            DistanceM = PatchField<int>.From(2000)
        };
        var command = new EditStorageRouteCommand(Guid.NewGuid(), patchDto);
        
        await Assert.ThrowsAsync<StorageRouteNotFound>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditStorageRoute_WithInvalidData_ThrowsValidationException()
    {
        await _mediator.AddMockStorageRoute(_storageFrom.Name, _storageTo.Name, _currency.Id);
        var route = await _context.StorageRoutes.FirstAsync();
        
        var patchDto = new PatchStorageRouteDto
        {
            DistanceM = PatchField<int>.From(0)
        };
        
        var command = new EditStorageRouteCommand(route.Id, patchDto);

        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task EditStorageRoute_WithInvalidPricePrecision_ThrowsValidationException()
    {
        await _mediator.AddMockStorageRoute(_storageFrom.Name, _storageTo.Name, _currency.Id);
        var route = await _context.StorageRoutes.FirstAsync();
        
        var patchDto = new PatchStorageRouteDto
        {
            PriceKg = PatchField<decimal>.From(10.555m)
        };
        
        var command = new EditStorageRouteCommand(route.Id, patchDto);
        
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }
}