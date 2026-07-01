using FluentAssertions;
using Main.Application.Handlers.ProductReservations.GetProductsWithNotEnoughStock;
using Main.Entities.Product;
using Main.Entities.Storage;
using Main.Entities.User;
using Main.Enums;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders.Storage;
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
            storage.Name,
            5);
        var query = Query(
            buyer.Id,
            storage.Name,
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
            storage.Name,
            2);
        var query = Query(
            buyer.Id,
            storage.Name,
            product.Id,
            5);

        var result = await Mediator.Send(query);

        result.NotEnoughByStock.Should()
            .ContainKey(product.Id)
            .WhoseValue.Should()
            .Be(3);
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
            storage.Name,
            5);
        await AddReservation(
            otherBuyer.Id,
            product.Id,
            2,
            1);
        var query = Query(
            buyer.Id,
            storage.Name,
            product.Id,
            5);

        var result = await Mediator.Send(query);

        result.NotEnoughByStock.Should().BeEmpty();
        result.NotEnoughByReservation.Should()
            .ContainKey(product.Id)
            .WhoseValue.Should()
            .Be(1);
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
            storage.Name,
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
            storage.Name,
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
        var otherStorage = OtherStorage(storage.Name);
        await AddStorageContent(
            product.Id,
            storage.Name,
            2);
        await AddStorageContent(
            product.Id,
            otherStorage.Name,
            4);
        var query = Query(
            buyer.Id,
            storage.Name,
            product.Id,
            5);

        var result = await Mediator.Send(query);

        result.NotEnoughByStock.Should()
            .ContainKey(product.Id)
            .WhoseValue.Should()
            .Be(3);
        result.NotEnoughByReservation.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenTakeFromOtherStoragesIsTrue_UsesAllStorages()
    {
        var buyer = Buyer();
        var product = Product();
        var storage = Storage();
        var otherStorage = OtherStorage(storage.Name);
        await AddStorageContent(
            product.Id,
            storage.Name,
            2);
        await AddStorageContent(
            product.Id,
            otherStorage.Name,
            4);
        var query = Query(
            buyer.Id,
            storage.Name,
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
            storage.Name,
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
            storage.Name,
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
            storage.Name,
            product.Id,
            1);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(query));
    }

    [Fact]
    public async Task WithEmptyStorageName_ThrowsValidationException()
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
            Storage().Name,
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
            Storage().Name,
            999999,
            1);

        await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(query));
    }

    private GetProductsWithNotEnoughStockQuery Query(
        Guid buyerId,
        string storageName,
        int productId,
        int neededCount,
        bool takeFromOtherStorages = false)
    {
        return new GetProductsWithNotEnoughStockQuery(
            buyerId,
            storageName,
            takeFromOtherStorages,
            new Dictionary<int, int>
            {
                [productId] = neededCount
            });
    }

    private async Task AddStorageContent(
        int productId,
        string storageName,
        int count)
    {
        await new StorageContentBuilder(Faker)
            .WithProductIds(productId)
            .WithStorageName(storageName)
            .WithCurrencyId(CurrencyContext.Currencies[0].Id)
            .WithCount(count)
            .BuildAndAddToDb(Context);
    }

    private async Task<StorageContentReservation> AddReservation(
        Guid userId,
        int productId,
        int reservedCount,
        int currentCount)
    {
        return await new StorageContentReservationBuilder(Faker)
            .WithUserId(userId)
            .WithProductId(productId)
            .WithReservedCount(reservedCount)
            .WithCurrentCount(currentCount)
            .BuildAndAddToDb(Context);
    }

    private User Buyer() { return UsersContext.Users.First(); }

    private User OtherBuyer(Guid buyerId) { return UsersContext.Users.First(x => x.Id != buyerId); }

    private Product Product() { return ProductContext.Products[0]; }

    private Storage Storage() { return StorageContext.Storages.First(x => x.Type == StorageType.Warehouse); }

    private Storage OtherStorage(string storageName)
    {
        return StorageContext.Storages.First(x => x.Name != storageName);
    }
}