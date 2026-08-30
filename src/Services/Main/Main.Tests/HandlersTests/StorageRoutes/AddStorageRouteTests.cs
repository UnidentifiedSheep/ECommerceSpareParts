using Main.Application.Handlers.StorageRoutes.AddStorageRoute;
using Main.Application.Static;
using Main.Entities.Currency;
using Main.Entities.Storage;
using Main.Entities.User;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders.User;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.StorageRoutes;

public class AddStorageRouteTests : IntegrationTest
{
	private User _carrier = null!;

	private Currency _currency = null!;

	private Storage _fromStorage = null!;

	private Storage _toStorage = null!;

	public AddStorageRouteTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<CurrencyRatesTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	public override async Task InitializeAsync()
	{
		await base.InitializeAsync();
		_carrier = await new MemberUserBuilder(Faker).BuildAndAddToDb(Context);

		_fromStorage = GetContext<StorageTestContext>().Storages.First(x => x.Type == StorageType.Warehouse);

		_toStorage = GetContext<StorageTestContext>()
			.Storages
			.First(x => x.Type == StorageType.SupplierStorage);

		_currency = GetContext<CurrencyTestContext>().Currencies[0];
	}

	[Fact]
	public async Task AddStorageRoute_WithValidData_Succeeds()
	{
		var command = new AddStorageRouteCommand(
			_toStorage.Code,
			_fromStorage.Code,
			1000,
			RouteType.IntraCity,
			LogisticPricingType.PerOrder,
			60,
			10.5m,
			20.5m,
			_currency.Id,
			5.0m,
			0,
			_carrier.Id);

		var result = await Mediator.Send(command);

		var route = await Context.StorageRoutes.FirstOrDefaultAsync(x => x.Id == result.RouteId);
		Assert.NotNull(route);
		Assert.Equal(_toStorage.Code, route.FromStorageCode);
		Assert.Equal(_fromStorage.Code, route.ToStorageCode);
		Assert.Equal(1000, route.DistanceM);
		Assert.Equal(10.5m, route.PriceKg);
	}

	[Fact]
	public async Task AddStorageRoute_WithInvalidDistance_ThrowsValidationException()
	{
		var command = new AddStorageRouteCommand(
			_fromStorage.Code,
			_toStorage.Code,
			0,
			RouteType.IntraCity,
			LogisticPricingType.PerOrder,
			60,
			10.5m,
			20.5m,
			_currency.Id,
			5.0m,
			0,
			_carrier.Id);

		await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
	}

	[Fact]
	public async Task AddStorageRoute_WithInvalidPricePrecision_ThrowsValidationException()
	{
		var command = new AddStorageRouteCommand(
			_fromStorage.Code,
			_toStorage.Code,
			1000,
			RouteType.IntraCity,
			LogisticPricingType.PerOrder,
			60,
			10.555m,
			20.5m,
			_currency.Id,
			5.0m,
			0,
			_carrier.Id);

		await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
	}

	[Fact]
	public async Task AddStorageRoute_WithNonExistentCurrency_ThrowsValidationException()
	{
		var command = new AddStorageRouteCommand(
			_fromStorage.Code,
			_toStorage.Code,
			1000,
			RouteType.IntraCity,
			LogisticPricingType.PerOrder,
			60,
			10.5m,
			20.5m,
			9999,
			5.0m,
			0,
			_carrier.Id);

		await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));
	}

	[Fact]
	public async Task AddStorageRoute_WithNonExistentStorage_ThrowsDbValidationException()
	{
		var command = new AddStorageRouteCommand(
			"NonExistentStorage",
			_toStorage.Code,
			1000,
			RouteType.IntraCity,
			LogisticPricingType.PerOrder,
			60,
			10.5m,
			20.5m,
			_currency.Id,
			5.0m,
			0,
			_carrier.Id);

		var exception =
			await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));
		Assert.Contains(exception.Failures, f => f.ErrorName == ApplicationErrors.StoragesNotFound);
	}
}
