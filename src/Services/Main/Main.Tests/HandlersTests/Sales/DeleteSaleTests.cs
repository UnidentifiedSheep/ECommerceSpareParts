using Exceptions;
using FluentAssertions;
using Main.Application.Handlers.Sales;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Enums;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts.Sale;

namespace Tests.HandlersTests.Sales;

public class DeleteSaleTests : IntegrationTest
{
    public DeleteSaleTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<SaleTestContext>();
    }

    private SaleTestContext SaleContext => GetContext<SaleTestContext>();

    [Fact]
    public async Task DeleteSale_ValidSale_MarksSaleDeletedRestoresStockAndReversesTransaction()
    {
        var sale = await ReloadSale();
        var storageContentId = sale.Contents.Single().Details.Single().StorageContentId;
        var productId = sale.Contents.Single().ProductId;
        var transactionId = sale.TransactionId;
        var soldCount = SaleContext.SoldCount;
        var storageCountBeforeDelete = await StorageContentCount(storageContentId);
        var productStockBeforeDelete = await ProductStock(productId);

        await Mediator.Send(new DeleteSaleCommand(sale.Id, sale.RowVersion));

        var deletedSale = await ReloadSale();
        var storageContent = await Context.StorageContents
            .AsNoTracking()
            .SingleAsync(x => x.Id == storageContentId);
        var product = await Context.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == productId);
        var transaction = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == transactionId);
        var movement = await Context.Events
            .OfType<StorageMovementEvent>()
            .AsNoTracking()
            .SingleAsync();

        deletedSale.State.Should().Be(SaleState.Deleted);
        storageContent.Count.Should().Be(storageCountBeforeDelete + soldCount);
        product.Stock.Value.Should().Be(productStockBeforeDelete + soldCount);
        transaction.IsReversed.Should().BeTrue();
        transaction.IsReversalApplied.Should().BeTrue();
        transaction.SourceType.Should().Be(TransactionSourceType.Sale);
        movement.Data.ProductId.Should().Be(productId);
        movement.Data.StorageName.Should().Be(storageContent.StorageName);
        movement.Data.MovementType.Should().Be(StorageMovementType.SaleDeletion);
    }

    [Fact]
    public async Task DeleteSale_WhenSaleAlreadyDeleted_DoesNothing()
    {
        var sale = await ReloadSale();
        var storageContentId = sale.Contents.Single().Details.Single().StorageContentId;
        var productId = sale.Contents.Single().ProductId;

        await Mediator.Send(new DeleteSaleCommand(sale.Id, sale.RowVersion));

        var storageCountAfterFirstDelete = await StorageContentCount(storageContentId);
        var productStockAfterFirstDelete = await ProductStock(productId);
        var movementsAfterFirstDelete = await Context.Events
            .OfType<StorageMovementEvent>()
            .AsNoTracking()
            .CountAsync();

        await Mediator.Send(new DeleteSaleCommand(sale.Id, sale.RowVersion));

        var deletedSale = await ReloadSale();
        var transaction = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == sale.TransactionId);
        var movementsAfterSecondDelete = await Context.Events
            .OfType<StorageMovementEvent>()
            .AsNoTracking()
            .CountAsync();

        deletedSale.State.Should().Be(SaleState.Deleted);
        (await StorageContentCount(storageContentId)).Should().Be(storageCountAfterFirstDelete);
        (await ProductStock(productId)).Should().Be(productStockAfterFirstDelete);
        movementsAfterSecondDelete.Should().Be(movementsAfterFirstDelete);
        transaction.IsReversed.Should().BeTrue();
        transaction.IsReversalApplied.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteSale_WhenSaleDoesNotExist_ThrowsSaleNotFoundException()
    {
        await Assert.ThrowsAsync<SaleNotFoundException>(() =>
            Mediator.Send(new DeleteSaleCommand(Guid.NewGuid(), 1)));
    }

    [Fact]
    public async Task DeleteSale_WhenRowVersionIsInvalid_ThrowsInvalidRowVersionException()
    {
        var sale = await ReloadSale();
        var invalidRowVersion = sale.RowVersion + 1;

        await Assert.ThrowsAsync<InvalidRowVersionException>(() =>
            Mediator.Send(new DeleteSaleCommand(sale.Id, invalidRowVersion)));

        var saleAfterFailedDelete = await ReloadSale();
        var transaction = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == sale.TransactionId);

        saleAfterFailedDelete.State.Should().Be(SaleState.Completed);
        transaction.IsReversed.Should().BeFalse();
        transaction.IsReversalApplied.Should().BeFalse();
    }

    private Task<Main.Entities.Sale.Sale> ReloadSale()
    {
        return Context.Sales
            .Include(x => x.Contents)
            .ThenInclude(x => x.Details)
            .AsNoTracking()
            .SingleAsync(x => x.Id == SaleContext.Sale.Id);
    }

    private async Task<int> StorageContentCount(int storageContentId)
    {
        var storageContent = await Context.StorageContents
            .AsNoTracking()
            .SingleAsync(x => x.Id == storageContentId);

        return storageContent.Count;
    }

    private async Task<int> ProductStock(int productId)
    {
        var product = await Context.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == productId);

        return product.Stock.Value;
    }
}
