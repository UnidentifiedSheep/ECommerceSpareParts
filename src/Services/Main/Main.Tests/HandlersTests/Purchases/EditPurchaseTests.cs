using FluentAssertions;
using Main.Application.Dtos.Purchase;
using Main.Application.Handlers.Purchases.EditPurchase;
using Main.Application.Handlers.StorageContents.SubtractContent;
using Main.Entities.Storage;
using Main.Enums;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Purchase;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Purchases;

public class EditPurchaseTests : IntegrationTest
{
    public EditPurchaseTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<PurchaseTestContext>();
        RegisterBasicContext<ProductMeasurementsTestContext>();
    }

    private PurchaseTestContext PurchaseContext => GetContext<PurchaseTestContext>();

    [Fact]
    public async Task EditPurchase_ValidContent_UpdatesPurchaseStorageAndTransaction()
    {
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var purchase = PurchaseContext.Purchase;
        var product = PurchaseContext.Product;
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
            purchase.Id,
            currency.Id,
            "edited purchase",
            DateTime.UtcNow,
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
        var products = GetContext<ProductTestContext>().Products.Take(2).ToList();
        var purchase = PurchaseContext.Purchase;
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
            purchase.Id,
            currency.Id,
            null,
            DateTime.UtcNow,
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
    public async Task
        EditPurchase_WhenPartOfStorageContentAlreadyUsed_DecreaseSubtractsOnlyDeltaFromRemainingStock()
    {
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var purchase = PurchaseContext.Purchase;
        var product = PurchaseContext.Product;
        var content = purchase.Contents.Single();
        var storageContentId = content.StorageContentId;
        content.SetCount(10);
        PurchaseContext.StorageContent.IncreaseCount(8);
        product.IncreaseStock(8);
        await Context.SaveChangesAsync();

        await Mediator.Send(
            new SubtractStorageContentsCommand(
                storageContentId,
                5,
                StorageMovementType.Sale));

        await Mediator.Send(
            new EditPurchaseCommand(
                [
                    new EditPurchaseDto
                    {
                        Id = content.Id,
                        ProductId = product.Id,
                        Count = 8,
                        Price = 10m
                    }
                ],
                purchase.Id,
                currency.Id,
                null,
                DateTime.UtcNow,
                false,
                null));

        var updatedPurchaseContent = await Context.PurchaseContents
            .AsNoTracking()
            .SingleAsync();
        var updatedStorageContent = await Context.StorageContents
            .AsNoTracking()
            .SingleAsync(x => x.Id == storageContentId);
        var updatedProduct = await Context.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == product.Id);

        updatedPurchaseContent.Count.Should().Be(8);
        updatedStorageContent.Count.Should().Be(3);
        updatedProduct.Stock.Value.Should().Be(3);
    }

    [Fact]
    public async Task EditPurchase_WithLogistics_CreatesPurchaseLogistics()
    {
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var purchase = PurchaseContext.Purchase;
        var product = PurchaseContext.Product;
        var content = purchase.Contents.Single();
        await AddStorageOwner(route, PurchaseContext.Supplier.Id);

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
            purchase.Id,
            currency.Id,
            null,
            DateTime.UtcNow,
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

        if (updatedPurchase.PurchaseLogistic.TransactionId is not null)
        {
            var logisticsTransaction = await Context.Transactions
                .AsNoTracking()
                .SingleAsync(x => x.Id == updatedPurchase.PurchaseLogistic.TransactionId);

            logisticsTransaction.SourceType.Should().Be(TransactionSourceType.Logistic);
        }
    }

    [Fact]
    public async Task EditPurchase_WhenLogisticsDisabled_ClearsPurchaseLogistics()
    {
        var currency = GetContext<CurrencyTestContext>().Currencies[0];
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var purchase = PurchaseContext.Purchase;
        var product = PurchaseContext.Product;
        var content = purchase.Contents.Single();
        content.SetLogistic(
            1m,
            1m,
            1m);
        purchase.SetPurchaseLogistic(
            route.Id,
            route.CurrencyId,
            route.PricingModel,
            route.RouteType,
            route.PriceKg,
            route.PricePerM3,
            route.PricePerOrder,
            route.MinimumPrice,
            null,
            false);
        await Context.SaveChangesAsync();

        await Mediator.Send(
            new EditPurchaseCommand(
                [
                    new EditPurchaseDto
                    {
                        Id = content.Id,
                        ProductId = product.Id,
                        Count = content.Count,
                        Price = content.Price,
                        CalculateLogistics = false
                    }
                ],
                purchase.Id,
                currency.Id,
                null,
                DateTime.UtcNow,
                false,
                null));

        var updatedPurchase = await Context.Purchases
            .Include(x => x.PurchaseLogistic)
            .Include(x => x.Contents)
            .ThenInclude(x => x.PurchaseContentLogistic)
            .AsNoTracking()
            .SingleAsync();

        updatedPurchase.PurchaseLogistic.Should().BeNull();
        updatedPurchase.Contents.Single().PurchaseContentLogistic.Should().BeNull();
    }

    [Fact]
    public async Task EditPurchase_WithDuplicateContentIds_ThrowsValidationException()
    {
        var command = new EditPurchaseCommand(
            [
                new EditPurchaseDto { Id = 1, ProductId = 1, Count = 1, Price = 10m },
                new EditPurchaseDto { Id = 1, ProductId = 1, Count = 1, Price = 10m }
            ],
            Guid.NewGuid(),
            GetContext<CurrencyTestContext>().Currencies[0].Id,
            null,
            DateTime.UtcNow,
            false,
            null);

        await Assert.ThrowsAsync<ValidationException>(() => Mediator.Send(command));
    }

    private async Task AddStorageOwner(StorageRoute route, Guid supplierId)
    {
        Context.StorageOwners.Add(StorageOwner.Create(route.FromStorageName, supplierId));
        await Context.SaveChangesAsync();
    }
}