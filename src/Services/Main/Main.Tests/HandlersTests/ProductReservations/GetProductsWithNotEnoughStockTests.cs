using FluentAssertions;
using Main.Application.Handlers.ProductReservations.GetProductsWithNotEnoughStock;
using Main.Entities.Product;
using Main.Entities.Storage;
using Main.Entities.User;
using Main.Enums;
using Tests.DataBuilders.Storage;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.ProductReservations;

public class GetProductsWithNotEnoughStockTests : IntegrationTest
{
	public GetProductsWithNotEnoughStockTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<UsersTestContext>();
		RegisterBasicContext<StorageTestContext>();
		RegisterBasicContext<CurrencyTestContext>();
	}

	private ProductTestContext ProductContext => GetContext<ProductTestContext>();

	private UsersTestContext UsersContext => GetContext<UsersTestContext>();

	private StorageTestContext StorageContext => GetContext<StorageTestContext>();

	private CurrencyTestContext CurrencyContext => GetContext<CurrencyTestContext>();

	[Fact]
	public async Task WhenStockIsEnoughAndNoReservations_ReturnsEmptyResult()
	{
		var buyer = Buyer();
		var product = Product();
		var storage = Storage();
		await AddStorageContent(
			product.Id,
			storage.Code,
			5);
		var query = Query(
			buyer.Id,
			storage.Code,
			product.Id,
			5);

		var result = await Mediator.Send(query);

		result.NotEnoughByStock.Should().BeEmpty();
		result.NotEnoughByReservation.Should().BeEmpty();
	}

	[Fact]
	public async Task WhenStockIsNotEnough_ReturnsStockShortage()
	{
		var buyer = Buyer();
		var product = Product();
		var storage = Storage();
		await AddStorageContent(
			product.Id,
			storage.Code,
			2);
		var query = Query(
			buyer.Id,
			storage.Code,
			product.Id,
			5);

		var result = await Mediator.Send(query);

		result.NotEnoughByStock.Should().ContainKey(product.Id).WhoseValue.Should().Be(3);
		result.NotEnoughByReservation.Should().BeEmpty();
	}

	[Fact]
	public async Task WhenOtherReservationsExceedFreeStock_ReturnsReservationShortage()
	{
		var buyer = Buyer();
		var otherBuyer = OtherBuyer(buyer.Id);
		var product = Product();
		var storage = Storage();
		await AddStorageContent(
			product.Id,
			storage.Code,
			5);
		await AddReservation(
			otherBuyer.Id,
			product.Id,
			2,
			1);
		var query = Query(
			buyer.Id,
			storage.Code,
			product.Id,
			5);

		var result = await Mediator.Send(query);

		result.NotEnoughByStock.Should().BeEmpty();
		result.NotEnoughByReservation.Should().ContainKey(product.Id).WhoseValue.Should().Be(1);
	}

	[Fact]
	public async Task WhenBuyerHasReservation_OffsetsOtherReservations()
	{
		var buyer = Buyer();
		var otherBuyer = OtherBuyer(buyer.Id);
		var product = Product();
		var storage = Storage();
		await AddStorageContent(
			product.Id,
			storage.Code,
			5);
		await AddReservation(
			otherBuyer.Id,
			product.Id,
			3,
			2);
		await AddReservation(
			buyer.Id,
			product.Id,
			3,
			2);
		var query = Query(
			buyer.Id,
			storage.Code,
			product.Id,
			5);

		var result = await Mediator.Send(query);

		result.NotEnoughByStock.Should().BeEmpty();
		result.NotEnoughByReservation.Should().BeEmpty();
	}

	[Fact]
	public async Task WhenTakeFromOtherStoragesIsFalse_UsesOnlyRequestedStorage()
	{
		var buyer = Buyer();
		var product = Product();
		var storage = Storage();
		var otherStorage = OtherStorage(storage.Code);
		await AddStorageContent(
			product.Id,
			storage.Code,
			2);
		await AddStorageContent(
			product.Id,
			otherStorage.Code,
			4);
		var query = Query(
			buyer.Id,
			storage.Code,
			product.Id,
			5);

		var result = await Mediator.Send(query);

		result.NotEnoughByStock.Should().ContainKey(product.Id).WhoseValue.Should().Be(3);
		result.NotEnoughByReservation.Should().BeEmpty();
	}

	[Fact]
	public async Task WhenTakeFromOtherStoragesIsTrue_UsesAllStorages()
	{
		var buyer = Buyer();
		var product = Product();
		var storage = Storage();
		var otherStorage = OtherStorage(storage.Code);
		await AddStorageContent(
			product.Id,
			storage.Code,
			2);
		await AddStorageContent(
			product.Id,
			otherStorage.Code,
			4);
		var query = Query(
			buyer.Id,
			storage.Code,
			product.Id,
			5,
			true);

		var result = await Mediator.Send(query);

		result.NotEnoughByStock.Should().BeEmpty();
		result.NotEnoughByReservation.Should().BeEmpty();
	}

	[Fact]
	public async Task DoneAndCanceledReservations_DoNotAffectShortage()
	{
		var buyer = Buyer();
		var otherBuyer = OtherBuyer(buyer.Id);
		var product = Product();
		var storage = Storage();
		await AddStorageContent(
			product.Id,
			storage.Code,
			5);
		await AddReservation(
			otherBuyer.Id,
			product.Id,
			2,
			2);
		var canceled = await AddReservation(
			otherBuyer.Id,
			product.Id,
			3,
			1);
		canceled.Cancel();
		await Context.SaveChangesAsync();
		var query = Query(
			buyer.Id,
			storage.Code,
			product.Id,
			5);

		var result = await Mediator.Send(query);

		result.NotEnoughByStock.Should().BeEmpty();
		result.NotEnoughByReservation.Should().BeEmpty();
	}

	[Fact]
	public async Task WithEmptyBuyerId_ThrowsValidationException()
	{
		var product = Product();
		var storage = Storage();
		var query = Query(
			Guid.Empty,
			storage.Code,
			product.Id,
			1);

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(query));
	}

	[Fact]
	public async Task WithEmptyStorageCode_ThrowsValidationException()
	{
		var query = Query(
			Buyer().Id,
			"",
			Product().Id,
			1);

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(query));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task WithInvalidNeededCount_ThrowsValidationException(int neededCount)
	{
		var query = Query(
			Buyer().Id,
			Storage().Code,
			Product().Id,
			neededCount);

		await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(query));
	}

	[Fact]
	public async Task WithMissingStorage_ThrowsDbValidationException()
	{
		var query = Query(
			Buyer().Id,
			"missing-storage",
			Product().Id,
			1);

		await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(query));
	}

	[Fact]
	public async Task WithMissingProduct_ThrowsDbValidationException()
	{
		var query = Query(
			Buyer().Id,
			Storage().Code,
			999999,
			1);

		await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(query));
	}

	private GetProductsWithNotEnoughStockQuery Query(
		Guid buyerId,
		string storageCode,
		int productId,
		int neededCount,
		bool takeFromOtherStorages = false)
	{
		return new GetProductsWithNotEnoughStockQuery(
			buyerId,
			storageCode,
			takeFromOtherStorages,
			new Dictionary<int, int>
			{
				[productId] = neededCount
			});
	}

	private async Task AddStorageContent(
		int productId,
		string storageCode,
		int count)
	{
		await new StorageContentBuilder(Faker)
			.WithProductIds(productId)
			.WithStorageCode(storageCode)
			.WithCurrencyId(CurrencyContext.Currencies[0].Id)
			.WithCount(count)
			.BuildAndAddToDb(Context);
	}

	private async Task<ProductReservation> AddReservation(
		Guid organizationId,
		int productId,
		int reservedCount,
		int currentCount)
	{
		return await new ProductReservationBuilder(Faker)
			.WithOrganizationId(organizationId)
			.WithProductId(productId)
			.WithReservedCount(reservedCount)
			.WithCurrentCount(currentCount)
			.BuildAndAddToDb(Context);
	}

	private User Buyer() => UsersContext.Users.First();

	private User OtherBuyer(Guid buyerId) => UsersContext.Users.First(x => x.Id != buyerId);

	private Product Product() => ProductContext.Products[0];

	private Storage Storage() => StorageContext.Storages.First(x => x.Type == StorageType.Warehouse);

	private Storage OtherStorage(string storageCode) =>
		StorageContext.Storages.First(x => x.Code != storageCode);
}
