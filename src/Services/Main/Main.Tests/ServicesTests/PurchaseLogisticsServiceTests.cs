using FluentAssertions;
using Main.Application.Interfaces.Services;
using Main.Entities.Purchase;
using Main.Enums;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders.Purchase;
using Tests.DataBuilders.Storage;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Purchase;
using Tests.TestContexts.Storage;

namespace Tests.ServicesTests;

public class PurchaseLogisticsServiceTests : IntegrationTest
{
    private IPurchaseLogisticsService _service = null!;

    public PurchaseLogisticsServiceTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<PurchaseTestContext>();
        RegisterBasicContext<ProductMeasurementsTestContext>();
    }

    private PurchaseTestContext PurchaseContext => GetContext<PurchaseTestContext>();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _service = Scope.ServiceProvider.GetRequiredService<IPurchaseLogisticsService>();
    }

    [Fact]
    public async Task ApplyAsync_WithItems_CreatesPurchaseAndContentLogistics()
    {
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var purchase = PurchaseContext.Purchase;

        await _service.ApplyAsync(
            purchase,
            [ToLogisticsItem(purchase.Contents.Single())],
            route.FromStorageName,
            DateTime.UtcNow,
            GetContext<UserContextTestContext>().SystemUser.Id);
        await Context.SaveChangesAsync();

        var updatedPurchase = await LoadPurchase();
        updatedPurchase.PurchaseLogistic.Should().NotBeNull();
        updatedPurchase.PurchaseLogistic!.RouteId.Should().Be(route.Id);
        updatedPurchase.Contents.Single().PurchaseContentLogistic.Should().NotBeNull();
    }

    [Fact]
    public async Task ApplyAsync_WithCarrier_CreatesLogisticsTransaction()
    {
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var purchase = PurchaseContext.Purchase;

        await _service.ApplyAsync(
            purchase,
            [ToLogisticsItem(purchase.Contents.Single())],
            route.FromStorageName,
            DateTime.UtcNow,
            GetContext<UserContextTestContext>().SystemUser.Id);
        await Context.SaveChangesAsync();

        var updatedPurchase = await LoadPurchase();
        updatedPurchase.PurchaseLogistic!.TransactionId.Should().NotBeNull();

        var transaction = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == updatedPurchase.PurchaseLogistic.TransactionId);
        transaction.SenderId.Should().Be(route.CarrierId!.Value);
        transaction.ReceiverId.Should().Be(GetContext<UserContextTestContext>().SystemUser.Id);
        transaction.SourceType.Should().Be(TransactionSourceType.Logistic);
    }

    [Fact]
    public async Task ApplyAsync_WithoutCarrier_DoesNotCreateLogisticsTransaction()
    {
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        route.SetCarrierId(null);
        await Context.SaveChangesAsync();

        var purchase = PurchaseContext.Purchase;

        await _service.ApplyAsync(
            purchase,
            [ToLogisticsItem(purchase.Contents.Single())],
            route.FromStorageName,
            DateTime.UtcNow,
            GetContext<UserContextTestContext>().SystemUser.Id);
        await Context.SaveChangesAsync();

        var updatedPurchase = await LoadPurchase();
        updatedPurchase.PurchaseLogistic!.TransactionId.Should().BeNull();
    }

    [Fact]
    public async Task ApplyAsync_WhenPurchaseAlreadyHasLogistics_UpdatesIt()
    {
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var oldRoute = GetContext<StorageRouteTestContext>().UnactiveRoute;
        var purchase = PurchaseContext.Purchase;
        var content = purchase.Contents.Single();
        content.SetLogistic(
            99m,
            99m,
            99m);
        purchase.SetPurchaseLogistic(
            oldRoute.Id,
            oldRoute.CurrencyId,
            LogisticPricingType.None,
            oldRoute.RouteType,
            0m,
            0m,
            0m,
            null,
            null,
            false);
        await Context.SaveChangesAsync();
        await _service.ApplyAsync(
            purchase,
            [ToLogisticsItem(content)],
            route.FromStorageName,
            DateTime.UtcNow,
            GetContext<UserContextTestContext>().SystemUser.Id);
        await Context.SaveChangesAsync();

        var updatedPurchase = await LoadPurchase();
        updatedPurchase.PurchaseLogistic!.RouteId.Should().Be(route.Id);
        updatedPurchase.Contents.Single().PurchaseContentLogistic!.WeightKg.Should().NotBe(99m);
    }

    [Fact]
    public async Task ApplyAsync_WhenSomeContentsExcluded_ClearsOnlyExcludedContentLogistics()
    {
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var purchase = PurchaseContext.Purchase;
        var firstContent = purchase.Contents.Single();
        var secondProduct = GetContext<ProductTestContext>().Products.Skip(1).First();
        var secondStorageContent = await new StorageContentBuilder(Faker)
            .WithStorageName(route.ToStorageName)
            .WithProductIds(secondProduct.Id)
            .WithCurrencyId(GetContext<CurrencyTestContext>().Currencies[0].Id)
            .WithCount(1)
            .WithBuyPrice(10m)
            .BuildAndAddToDb(Context);
        var secondContent = new PurchaseContentBuilder(Faker)
            .WithProductId(secondProduct.Id)
            .WithCount(1)
            .WithPrice(10m)
            .WithStorageContentId(secondStorageContent.Id)
            .Build();
        purchase.AddContent(secondContent);
        await Context.SaveChangesAsync();

        await _service.ApplyAsync(
            purchase,
            [ToLogisticsItem(firstContent), ToLogisticsItem(secondContent)],
            route.FromStorageName,
            DateTime.UtcNow,
            GetContext<UserContextTestContext>().SystemUser.Id);
        await Context.SaveChangesAsync();

        await _service.ApplyAsync(
            purchase,
            [ToLogisticsItem(firstContent)],
            route.FromStorageName,
            DateTime.UtcNow,
            GetContext<UserContextTestContext>().SystemUser.Id);
        await Context.SaveChangesAsync();

        var updatedPurchase = await LoadPurchase();
        updatedPurchase.Contents.Single(x => x.Id == firstContent.Id)
            .PurchaseContentLogistic.Should()
            .NotBeNull();
        updatedPurchase.Contents.Single(x => x.Id == secondContent.Id)
            .PurchaseContentLogistic.Should()
            .BeNull();
    }

    [Fact]
    public async Task ApplyAsync_WithoutItems_ClearsPurchaseAndContentLogistics()
    {
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var purchase = PurchaseContext.Purchase;

        await _service.ApplyAsync(
            purchase,
            [ToLogisticsItem(purchase.Contents.Single())],
            route.FromStorageName,
            DateTime.UtcNow,
            GetContext<UserContextTestContext>().SystemUser.Id);
        await Context.SaveChangesAsync();

        await _service.ApplyAsync(
            purchase,
            [],
            null,
            DateTime.UtcNow,
            GetContext<UserContextTestContext>().SystemUser.Id);
        await Context.SaveChangesAsync();

        var updatedPurchase = await LoadPurchase();
        updatedPurchase.PurchaseLogistic.Should().BeNull();
        updatedPurchase.Contents.Single().PurchaseContentLogistic.Should().BeNull();
    }

    [Fact]
    public async Task ApplyAsync_WhenItemsExistAndStorageFromMissing_Throws()
    {
        var purchase = PurchaseContext.Purchase;

        var act = () => _service.ApplyAsync(
            purchase,
            [ToLogisticsItem(purchase.Contents.Single())],
            null,
            DateTime.UtcNow,
            GetContext<UserContextTestContext>().SystemUser.Id);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private async Task<Purchase> LoadPurchase()
    {
        return await Context.Purchases
            .Include(x => x.PurchaseLogistic)
            .Include(x => x.Contents)
            .ThenInclude(x => x.PurchaseContentLogistic)
            .AsNoTracking()
            .SingleAsync();
    }

    private static PurchaseLogisticsItem ToLogisticsItem(PurchaseContent content)
    {
        return new PurchaseLogisticsItem(
            content,
            content.ProductId,
            content.Count);
    }
}