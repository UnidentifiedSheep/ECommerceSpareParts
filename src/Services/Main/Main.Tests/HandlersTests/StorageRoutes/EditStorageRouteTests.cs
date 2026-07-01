using Abstractions.Models;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.StorageRoutes.EditStorageRoute;
using Main.Entities.Exceptions;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.StorageRoutes;

public class EditStorageRouteTests : IntegrationTest
{
    private StorageRouteTestContext _testContext = null!;

    public EditStorageRouteTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<StorageRouteTestContext>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _testContext = GetContext<StorageRouteTestContext>();
    }

    [Fact]
    public async Task EditStorageRoute_WithValidData_Succeeds()
    {
        var route = _testContext.ActiveRoute;
        var patchDto = new PatchStorageRouteDto
        {
            DistanceM = PatchField<int>.From(2000),
            PriceKg = PatchField<decimal>.From(15.5m),
            IsActive = PatchField<bool>.From(false)
        };

        var command = new EditStorageRouteCommand(_testContext.ActiveRoute.Id, patchDto);

        await Mediator.Send(command);

        var updatedRoute = await Context.StorageRoutes.AsNoTracking().FirstAsync(x => x.Id == route.Id);
        Assert.Equal(2000, updatedRoute.DistanceM);
        Assert.Equal(15.5m, updatedRoute.PriceKg);
        Assert.False(updatedRoute.IsActive);

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

        await Assert.ThrowsAsync<StorageRouteNotFound>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task EditStorageRoute_WithInvalidData_ThrowsValidationException()
    {
        var patchDto = new PatchStorageRouteDto
        {
            DistanceM = PatchField<int>.From(0)
        };

        var command = new EditStorageRouteCommand(_testContext.ActiveRoute.Id, patchDto);

        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task EditStorageRoute_WithInvalidPricePrecision_ThrowsValidationException()
    {
        var patchDto = new PatchStorageRouteDto
        {
            PriceKg = PatchField<decimal>.From(10.555m)
        };

        var command = new EditStorageRouteCommand(_testContext.ActiveRoute.Id, patchDto);

        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }
}