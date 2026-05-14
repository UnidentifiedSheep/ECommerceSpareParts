using Enums;
using FluentAssertions;
using Main.Application.Dtos.Amw.Purchase;
using Main.Application.Handlers.Purchases.CreateFullPurchase;
using Main.Application.Handlers.Purchases.EditFullPurchase;
using Main.Entities.Product;
using Main.Entities.Storage;
using Main.Entities.User;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders.User;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;
using ValidationException = FluentValidation.ValidationException;

namespace Tests.HandlersTests.Purchases;

public class EditPurchaseTests : IntegrationTest
{
    private User _supplier = null!;

    public EditPurchaseTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<ProductTestContext>();
        RegisterBasicContext<CurrencyRatesTestContext>();
        RegisterBasicContext<StorageTestContext>();
        RegisterBasicContext<StorageRouteTestContext>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _supplier = await new SupplierUserBuilder(Faker)
            .BuildAndAddToDb(Context);
    }

    [Fact]
    public async Task EditPurchase_ValidContent_UpdatesPurchaseStorageAndTransaction()
    {
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var storage = GetContext<StorageTestContext>().Storages.First();
        var product = GetContext<ProductTestContext>().Products.First();
        await Mediator.Send(CreateCommand(
            currency.Id,
            storage.Name,
            [NewContent(product, count: 2)]));

        var purchase = await Context.Purchases
            .Include(x => x.Contents)
            .SingleAsync();
        var content = purchase.Contents.Single();
        var oldTransactionId = purchase.TransactionId;

        var editCommand = new EditPurchaseCommand(
            [
                new EditPurchaseDto
                {
                    Id = content.Id,
                    ProductId = product.Id,
                    Count = 5,
                    Price = 20m,
                    Comment = "updated"
                }
            ],
            purchase.Id.ToString(),
            currency.Id,
            "edited purchase",
            DateTime.UtcNow,
            GetContext<UserContextTestContext>().SystemUser.Id,
            false,
            null);

        await Mediator.Send(editCommand);

        var updatedPurchase = await Context.Purchases
            .Include(x => x.Contents)
            .AsNoTracking()
            .SingleAsync();
        var updatedContent = updatedPurchase.Contents.Single();
        var storageContent = await Context.StorageContents
            .AsNoTracking()
            .SingleAsync(x => x.Id == updatedContent.StorageContentId);
        var oldTransaction = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == oldTransactionId);
        var newTransaction = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == updatedPurchase.TransactionId);
        var updatedProduct = await Context.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == product.Id);

        updatedPurchase.Comment.Should().Be(editCommand.Comment);
        updatedPurchase.CurrencyId.Should().Be(currency.Id);
        updatedContent.Count.Should().Be(5);
        updatedContent.Price.Should().Be(20m);
        updatedContent.TotalSum.Should().Be(100m);
        updatedContent.Comment.Should().Be("updated");
        storageContent.Count.Should().Be(5);
        storageContent.BuyPrice.Should().Be(20m);
        oldTransaction.IsReversed.Should().BeTrue();
        oldTransaction.IsReversalApplied.Should().BeTrue();
        newTransaction.Amount.Should().Be(100m);
        updatedProduct.Stock.Value.Should().Be(5);
    }

    [Fact]
    public async Task EditPurchase_AddAndRemoveContent_UpdatesPurchaseAndStorage()
    {
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var storage = GetContext<StorageTestContext>().Storages.First();
        var products = GetContext<ProductTestContext>().Products.Take(2).ToList();
        await Mediator.Send(CreateCommand(
            currency.Id,
            storage.Name,
            [NewContent(products[0], count: 2)]));

        var purchase = await Context.Purchases
            .Include(x => x.Contents)
            .SingleAsync();
        var removedStorageContentId = purchase.Contents.Single().StorageContentId;

        var editCommand = new EditPurchaseCommand(
            [
                new EditPurchaseDto
                {
                    ProductId = products[1].Id,
                    Count = 3,
                    Price = 15m
                }
            ],
            purchase.Id.ToString(),
            currency.Id,
            null,
            DateTime.UtcNow,
            GetContext<UserContextTestContext>().SystemUser.Id,
            false,
            null);

        await Mediator.Send(editCommand);

        var updatedPurchase = await Context.Purchases
            .Include(x => x.Contents)
            .AsNoTracking()
            .SingleAsync();
        var contents = await Context.StorageContents.AsNoTracking().ToListAsync();
        var removedProduct = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == products[0].Id);
        var addedProduct = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == products[1].Id);

        updatedPurchase.Contents.Should().ContainSingle(x => x.ProductId == products[1].Id);
        contents.Single(x => x.Id == removedStorageContentId).Count.Should().Be(0);
        contents.Single(x => x.ProductId == products[1].Id).Count.Should().Be(3);
        removedProduct.Stock.Value.Should().Be(0);
        addedProduct.Stock.Value.Should().Be(3);
    }

    [Fact]
    public async Task EditPurchase_WithLogistics_CreatesPurchaseLogistics()
    {
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var product = GetContext<ProductTestContext>().Products.First();
        await Mediator.Send(CreateCommand(
            currency.Id,
            route.ToStorageName,
            [NewContent(product, count: 2)]));

        var purchase = await Context.Purchases
            .Include(x => x.Contents)
            .SingleAsync();
        var content = purchase.Contents.Single();
        await AddLogisticsDependencies(product, route, _supplier.Id);

        var editCommand = new EditPurchaseCommand(
            [
                new EditPurchaseDto
                {
                    Id = content.Id,
                    ProductId = product.Id,
                    Count = 2,
                    Price = 10m,
                    CalculateLogistics = true
                }
            ],
            purchase.Id.ToString(),
            currency.Id,
            null,
            DateTime.UtcNow,
            GetContext<UserContextTestContext>().SystemUser.Id,
            true,
            route.FromStorageName);

        await Mediator.Send(editCommand);

        var updatedPurchase = await Context.Purchases
            .Include(x => x.PurchaseLogistic)
            .Include(x => x.Contents)
            .ThenInclude(x => x.PurchaseContentLogistic)
            .AsNoTracking()
            .SingleAsync();

        updatedPurchase.PurchaseLogistic.Should().NotBeNull();
        updatedPurchase.PurchaseLogistic!.RouteId.Should().Be(route.Id);
        updatedPurchase.Contents.Single().PurchaseContentLogistic.Should().NotBeNull();
    }

    [Fact]
    public async Task EditPurchase_WithDuplicateContentIds_ThrowsValidationException()
    {
        var command = new EditPurchaseCommand(
            [
                new EditPurchaseDto { Id = 1, ProductId = 1, Count = 1, Price = 10m },
                new EditPurchaseDto { Id = 1, ProductId = 1, Count = 1, Price = 10m }
            ],
            Guid.NewGuid().ToString(),
            GetContext<CurrencyTestContext>().Currencies[0].Id,
            null,
            DateTime.UtcNow,
            GetContext<UserContextTestContext>().SystemUser.Id,
            false,
            null);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    private CreatePurchaseCommand CreateCommand(
        int currencyId,
        string storageName,
        IEnumerable<NewPurchaseContentDto> contents)
    {
        return new CreatePurchaseCommand(
            GetContext<UserContextTestContext>().SystemUser.Id,
            _supplier.Id,
            currencyId,
            storageName,
            DateTime.UtcNow,
            contents.ToList(),
            null,
            null,
            false,
            null);
    }

    private static NewPurchaseContentDto NewContent(Product product, int count)
    {
        return new NewPurchaseContentDto
        {
            ProductId = product.Id,
            Count = count,
            Price = 10m,
            CalculateLogistics = false
        };
    }

    private async Task AddLogisticsDependencies(Product product, StorageRoute route, Guid supplierId)
    {
        Context.ProductSizes.Add(ProductSize.Create(product.Id, 1m, 1m, 1m, DimensionUnit.Meter));
        Context.ProductWeights.Add(ProductWeight.Create(product.Id, 2m, WeightUnit.Kilogram));
        Context.StorageOwners.Add(StorageOwner.Create(route.FromStorageName, supplierId));
        await Context.SaveChangesAsync();
    }
}
