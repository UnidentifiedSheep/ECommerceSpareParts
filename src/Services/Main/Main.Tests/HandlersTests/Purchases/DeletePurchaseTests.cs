using FluentAssertions;
using Main.Application.Handlers.Purchases.DeletePurchase;
using Main.Application.Handlers.StorageContents.SubtractContent;
using Main.Entities.Exceptions;
using Main.Enums;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Tests.DataBuilders.Balance;
using Tests.DataBuilders.Purchase;
using Tests.DataBuilders.Storage;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Purchase;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.Purchases;

public class DeletePurchaseTests : IntegrationTest
{
    public DeletePurchaseTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<PurchaseTestContext>();
        RegisterBasicContext<ProductMeasurementsTestContext>();
    }

    private PurchaseTestContext PurchaseContext => GetContext<PurchaseTestContext>();

    [Fact]
    public async Task DeletePurchase_ValidPurchase_RemovesPurchaseAndRevertsStockAndTransaction()
    {
        var purchase = PurchaseContext.Purchase;
        var content = purchase.Contents.Single();
        var storageContentId = content.StorageContentId;
        var productId = content.ProductId;
        var transactionId = purchase.TransactionId;

        await Mediator.Send(new DeletePurchaseCommand(purchase.Id));

        var purchaseExists = await Context.Purchases
            .AsNoTracking()
            .AnyAsync(x => x.Id == purchase.Id);
        var purchaseContentExists = await Context.PurchaseContents
            .AsNoTracking()
            .AnyAsync(x => x.PurchaseId == purchase.Id);
        var storageContent = await Context.StorageContents
            .AsNoTracking()
            .SingleAsync(x => x.Id == storageContentId);
        var product = await Context.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == productId);
        var transaction = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == transactionId);

        purchaseExists.Should().BeFalse();
        purchaseContentExists.Should().BeFalse();
        storageContent.Count.Should().Be(0);
        product.Stock.Value.Should().Be(0);
        transaction.IsReversed.Should().BeTrue();
        transaction.IsReversalApplied.Should().BeTrue();
    }

    [Fact]
    public async Task DeletePurchase_WithLogisticsTransaction_RevertsPurchaseAndLogisticsTransactions()
    {
        var route = GetContext<StorageRouteTestContext>().ActiveRoute;
        var currencyId = GetContext<CurrencyTestContext>().Currencies[0].Id;
        var systemUserId = GetContext<UserContextTestContext>().SystemUser.Id;
        var supplier = PurchaseContext.Supplier;
        var product = PurchaseContext.Product;
        var originalStock = product.Stock.Value;

        var supplierBalance = await Context.UserBalances
            .SingleAsync(x => x.OrganizationId == supplier.Id && x.CurrencyId == currencyId);
        var systemBalance = await Context.UserBalances
            .SingleAsync(x => x.OrganizationId == systemUserId && x.CurrencyId == currencyId);
        var carrierBalance = new UserBalanceBuilder(Faker)
            .WithUserId(route.CarrierId!.Value)
            .WithCurrencyId(currencyId)
            .Build();

        var purchaseTransaction = new TransactionBuilder(Faker)
            .WithSenderId(supplier.Id)
            .WithReceiverId(systemUserId)
            .WithCurrencyId(currencyId)
            .WithAmount(20m)
            .WithSourceType(TransactionSourceType.Purchase)
            .WithBalances(supplierBalance, systemBalance)
            .Completed()
            .Applied()
            .Build();
        var logisticsTransaction = new TransactionBuilder(Faker)
            .WithSenderId(route.CarrierId.Value)
            .WithReceiverId(systemUserId)
            .WithCurrencyId(currencyId)
            .WithAmount(5m)
            .WithSourceType(TransactionSourceType.Logistic)
            .WithBalances(carrierBalance, systemBalance)
            .Completed()
            .Applied()
            .Build();

        await Context.AddRangeAsync(
            carrierBalance,
            purchaseTransaction,
            logisticsTransaction);
        await Context.SaveChangesAsync();

        var storageContent = await new StorageContentBuilder(Faker)
            .WithStorageName(route.ToStorageName)
            .WithProductIds(product.Id)
            .WithCurrencyId(currencyId)
            .WithCount(2)
            .WithBuyPrice(10m)
            .BuildAndAddToDb(Context);
        product.IncreaseStock(storageContent.Count);

        var purchaseContent = new PurchaseContentBuilder(Faker)
            .WithProductId(product.Id)
            .WithCount(2)
            .WithPrice(10m)
            .WithStorageContentId(storageContent.Id)
            .Build();
        purchaseContent.SetLogistic(
            1m,
            1m,
            5m);

        var purchase = await new PurchaseBuilder(Faker)
            .WithSupplierUserId(supplier.Id)
            .WithSupplierOrganizationId(supplier.Id)
            .WithCurrencyId(currencyId)
            .WithTransactionId(purchaseTransaction.Id)
            .WithStorage(route.ToStorageName)
            .WithContent(purchaseContent)
            .WithLogistic(route, logisticsTransaction.Id)
            .BuildAndAddToDb(Context);

        await Mediator.Send(new DeletePurchaseCommand(purchase.Id));

        var purchaseExists = await Context.Purchases
            .AsNoTracking()
            .AnyAsync(x => x.Id == purchase.Id);
        var purchaseTransactionAfterDelete = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == purchaseTransaction.Id);
        var logisticsTransactionAfterDelete = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == logisticsTransaction.Id);
        var productAfterDelete = await Context.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == product.Id);

        purchaseExists.Should().BeFalse();
        purchaseTransactionAfterDelete.IsReversed.Should().BeTrue();
        purchaseTransactionAfterDelete.IsReversalApplied.Should().BeTrue();
        purchaseTransactionAfterDelete.SourceType.Should().Be(TransactionSourceType.Purchase);
        logisticsTransactionAfterDelete.IsReversed.Should().BeTrue();
        logisticsTransactionAfterDelete.IsReversalApplied.Should().BeTrue();
        logisticsTransactionAfterDelete.SourceType.Should().Be(TransactionSourceType.Logistic);
        productAfterDelete.Stock.Value.Should().Be(originalStock);
    }

    [Fact]
    public async Task DeletePurchase_WhenPurchaseIdIsEmpty_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(new DeletePurchaseCommand(Guid.Empty)));
    }

    [Fact]
    public async Task DeletePurchase_WhenPurchaseDoesNotExist_ThrowsPurchaseNotFoundException()
    {
        var purchaseId = Guid.NewGuid();

        await Assert.ThrowsAsync<PurchaseNotFoundException>(() =>
            Mediator.Send(new DeletePurchaseCommand(purchaseId)));
    }

    [Fact]
    public async Task DeletePurchase_WhenPurchasedStockAlreadyPartiallyUsed_DoesNotPersistPartialChanges()
    {
        var purchase = PurchaseContext.Purchase;
        var content = purchase.Contents.Single();
        var storageContentId = content.StorageContentId;
        var transactionId = purchase.TransactionId;

        await Mediator.Send(
            new SubtractStorageContentsCommand(
                storageContentId,
                1,
                StorageMovementType.Sale));

        await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() =>
            Mediator.Send(new DeletePurchaseCommand(purchase.Id)));

        var purchaseExists = await Context.Purchases
            .AsNoTracking()
            .AnyAsync(x => x.Id == purchase.Id);
        var storageContent = await Context.StorageContents
            .AsNoTracking()
            .SingleAsync(x => x.Id == storageContentId);
        var product = await Context.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == content.ProductId);
        var transaction = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == transactionId);

        purchaseExists.Should().BeTrue();
        storageContent.Count.Should().Be(1);
        product.Stock.Value.Should().Be(1);
        transaction.IsReversed.Should().BeFalse();
        transaction.IsReversalApplied.Should().BeFalse();
    }
}
