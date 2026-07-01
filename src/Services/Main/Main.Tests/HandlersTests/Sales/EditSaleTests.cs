using Exceptions;
using FluentAssertions;
using Main.Application.Dtos.Sale;
using Main.Application.Handlers.Sales.EditSale;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Entities.Sale;
using Main.Entities.Storage;
using Main.Enums;
using Main.Enums.Balances;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts.Sale;

namespace Tests.HandlersTests.Sales;

public class EditSaleTests : IntegrationTest
{
    public EditSaleTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<SaleTestContext>();
    }

    private SaleTestContext SaleContext => GetContext<SaleTestContext>();

    [Fact]
    public async Task EditSale_ValidSale_UpdatesSaleContentsAndRecreatesTransaction()
    {
        var sale = await ReloadSale();
        var content = sale.Contents.Single();
        var oldTransactionId = sale.TransactionId;
        var newDate = DateTime.UtcNow.AddMinutes(-5);
        var command = new EditSaleCommand(
            sale.Id,
            sale.RowVersion,
            [
                new EditSaleContentDto
                {
                    Id = content.Id,
                    ProductId = content.ProductId,
                    Count = content.Count,
                    Price = 120m,
                    PriceWithDiscount = 90m,
                    Comment = "edited"
                }
            ],
            sale.CurrencyId,
            newDate,
            "sale edited",
            null);

        await Mediator.Send(command);

        var editedSale = await ReloadSale();
        var editedContent = editedSale.Contents.Should().ContainSingle().Subject;
        var oldTransaction = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == oldTransactionId);
        var newTransaction = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == editedSale.TransactionId);
        var movements = await Context.Events
            .OfType<StorageMovementEvent>()
            .AsNoTracking()
            .ToListAsync();

        editedSale.Comment.Should().Be(command.Comment);
        editedSale.SaleDatetime.Should().BeCloseTo(newDate, TimeSpan.FromMilliseconds(1));
        editedSale.TransactionId.Should().NotBe(oldTransactionId);
        editedContent.Id.Should().Be(content.Id);
        editedContent.Price.Should().Be(90m);
        editedContent.TotalSum.Should().Be(90m);
        editedContent.Discount.Should().Be(0.25m);
        editedContent.Comment.Should().Be("edited");
        editedContent.Details.Sum(x => x.Count).Should().Be(content.Count);

        oldTransaction.IsReversed.Should().BeTrue();
        oldTransaction.IsReversalApplied.Should().BeTrue();
        newTransaction.Amount.Should().Be(90m);
        newTransaction.SourceType.Should().Be(TransactionSourceType.Sale);
        movements.Select(x => x.Data.MovementType)
            .Should()
            .BeEquivalentTo([StorageMovementType.SaleEditing, StorageMovementType.SaleEditing]);
    }

    [Fact]
    public async Task EditSale_WhenRowVersionIsInvalid_ThrowsInvalidRowVersionException()
    {
        var sale = await ReloadSale();
        var content = sale.Contents.Single();
        var command = new EditSaleCommand(
            sale.Id,
            sale.RowVersion + 1,
            [
                new EditSaleContentDto
                {
                    Id = content.Id,
                    ProductId = content.ProductId,
                    Count = content.Count,
                    Price = 120m,
                    PriceWithDiscount = 90m
                }
            ],
            sale.CurrencyId,
            sale.SaleDatetime,
            sale.Comment,
            null);

        await Assert.ThrowsAsync<InvalidRowVersionException>(() => Mediator.Send(command));

        var saleAfterFailedEdit = await ReloadSale();
        var transaction = await Context.Transactions
            .AsNoTracking()
            .SingleAsync(x => x.Id == sale.TransactionId);

        saleAfterFailedEdit.TransactionId.Should().Be(sale.TransactionId);
        transaction.IsReversed.Should().BeFalse();
    }

    [Fact]
    public async Task EditSale_WhenSaleDoesNotExist_ThrowsSaleNotFoundException()
    {
        await Assert.ThrowsAsync<SaleNotFoundException>(() =>
            Mediator.Send(
                new EditSaleCommand(
                    Guid.NewGuid(),
                    1,
                    [
                        new EditSaleContentDto
                        {
                            ProductId = SaleContext.Product.Id,
                            Count = 1,
                            Price = 100m,
                            PriceWithDiscount = 90m
                        }
                    ],
                    SaleContext.StorageContent.CurrencyId,
                    DateTime.UtcNow,
                    null,
                    null)));
    }

    [Fact]
    public async Task EditSale_AddsNewContent()
    {
        var sale = await ReloadSale();
        var content = sale.Contents.Single();
        var newStorageContent = await OtherStorageContent();
        var command = new EditSaleCommand(
            sale.Id,
            sale.RowVersion,
            [
                ToDto(content),
                NewDto(newStorageContent.ProductId)
            ],
            sale.CurrencyId,
            sale.SaleDatetime,
            sale.Comment,
            null);

        await Mediator.Send(command);

        var editedSale = await ReloadSale();
        editedSale.Contents.Should().HaveCount(2);
        editedSale.Contents.Should().Contain(x => x.Id == content.Id);
        editedSale.Contents.Should().Contain(x => x.ProductId == newStorageContent.ProductId);
    }

    [Fact]
    public async Task EditSale_RemovesOmittedContent()
    {
        var sale = await ReloadSale();
        var oldContent = sale.Contents.Single();
        var newStorageContent = await OtherStorageContent();
        var command = new EditSaleCommand(
            sale.Id,
            sale.RowVersion,
            [NewDto(newStorageContent.ProductId)],
            sale.CurrencyId,
            sale.SaleDatetime,
            sale.Comment,
            null);

        await Mediator.Send(command);

        var editedSale = await ReloadSale();
        editedSale.Contents.Should().ContainSingle();
        editedSale.Contents.Should().NotContain(x => x.Id == oldContent.Id);
        editedSale.Contents.Single().ProductId.Should().Be(newStorageContent.ProductId);
    }

    [Fact]
    public async Task EditSale_WhenContentIdDoesNotExist_ThrowsSaleContentNotFoundException()
    {
        var sale = await ReloadSale();
        var command = new EditSaleCommand(
            sale.Id,
            sale.RowVersion,
            [
                new EditSaleContentDto
                {
                    Id = int.MaxValue,
                    ProductId = SaleContext.Product.Id,
                    Count = 1,
                    Price = 100m,
                    PriceWithDiscount = 90m
                }
            ],
            sale.CurrencyId,
            sale.SaleDatetime,
            sale.Comment,
            null);

        await Assert.ThrowsAsync<SaleContentNotFoundException>(() => Mediator.Send(command));

        var saleAfterFailedEdit = await ReloadSale();
        saleAfterFailedEdit.TransactionId.Should().Be(sale.TransactionId);
    }

    [Fact]
    public async Task EditSale_WhenContentProductChanged_ThrowsArticleDoesntMatchContentException()
    {
        var sale = await ReloadSale();
        var content = sale.Contents.Single();
        var otherStorageContent = await OtherStorageContent();
        var command = new EditSaleCommand(
            sale.Id,
            sale.RowVersion,
            [
                new EditSaleContentDto
                {
                    Id = content.Id,
                    ProductId = otherStorageContent.ProductId,
                    Count = 1,
                    Price = 100m,
                    PriceWithDiscount = 90m
                }
            ],
            sale.CurrencyId,
            sale.SaleDatetime,
            sale.Comment,
            null);

        await Assert.ThrowsAsync<ArticleDoesntMatchContentException>(() => Mediator.Send(command));

        var saleAfterFailedEdit = await ReloadSale();
        saleAfterFailedEdit.TransactionId.Should().Be(sale.TransactionId);
    }

    [Fact]
    public async Task EditSale_WhenSaleDeleted_ThrowsSaleNotFoundException()
    {
        var sale = await Context.Sales.SingleAsync(x => x.Id == SaleContext.Sale.Id);
        sale.Delete();
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        var deletedSale = await ReloadSale();
        var content = deletedSale.Contents.Single();
        var command = new EditSaleCommand(
            deletedSale.Id,
            deletedSale.RowVersion,
            [ToDto(content)],
            deletedSale.CurrencyId,
            deletedSale.SaleDatetime,
            deletedSale.Comment,
            null);

        await Assert.ThrowsAsync<SaleNotFoundException>(() => Mediator.Send(command));
    }

    [Fact]
    public async Task EditSale_WhenCountIncreased_UpdatesStorageContentAndProductStock()
    {
        var sale = await ReloadSale();
        var content = sale.Contents.Single();
        var detail = content.Details.Single();
        await AddAvailableCount(
            content.ProductId,
            detail.StorageContentId,
            1);
        var storageCountBeforeEdit = await StorageProductCount(content.ProductId, sale.StorageName);
        var productStockBeforeEdit = await ProductStock(content.ProductId);
        var command = new EditSaleCommand(
            sale.Id,
            sale.RowVersion,
            [
                new EditSaleContentDto
                {
                    Id = content.Id,
                    ProductId = content.ProductId,
                    Count = content.Count + 1,
                    Price = 120m,
                    PriceWithDiscount = 90m
                }
            ],
            sale.CurrencyId,
            sale.SaleDatetime,
            sale.Comment,
            null);

        await Mediator.Send(command);

        var editedSale = await ReloadSale();
        var editedContent = editedSale.Contents.Single();
        var soldCount = editedContent.Count;
        editedContent.Count.Should().Be(content.Count + 1);
        editedContent.Details.Sum(x => x.Count).Should().Be(soldCount);
        (await ProductStock(content.ProductId)).Should().Be(productStockBeforeEdit - 1);
        (await StorageProductCount(content.ProductId, sale.StorageName)).Should()
            .Be(storageCountBeforeEdit - 1);
    }

    private Task<Sale> ReloadSale()
    {
        return Context.Sales
            .Include(x => x.Contents)
            .ThenInclude(x => x.Details)
            .AsNoTracking()
            .SingleAsync(x => x.Id == SaleContext.Sale.Id);
    }

    private static EditSaleContentDto ToDto(SaleContent content)
    {
        return new EditSaleContentDto
        {
            Id = content.Id,
            ProductId = content.ProductId,
            Count = content.Count,
            Price = content.Price,
            PriceWithDiscount = content.Price,
            Comment = content.Comment
        };
    }

    private static EditSaleContentDto NewDto(int productId)
    {
        return new EditSaleContentDto
        {
            ProductId = productId,
            Count = 1,
            Price = 100m,
            PriceWithDiscount = 90m
        };
    }

    private Task<StorageContent> OtherStorageContent()
    {
        return Context.StorageContents
            .AsNoTracking()
            .Where(x => x.Count > 0)
            .Where(x => x.StorageName == SaleContext.StorageContent.StorageName)
            .Where(x => x.ProductId != SaleContext.Product.Id)
            .FirstAsync();
    }

    private Task<int> StorageProductCount(int productId, string storageName)
    {
        return Context.StorageContents
            .AsNoTracking()
            .Where(x => x.ProductId == productId && x.StorageName == storageName)
            .SumAsync(x => x.Count);
    }

    private async Task<int> ProductStock(int productId)
    {
        var product = await Context.Products
            .AsNoTracking()
            .SingleAsync(x => x.Id == productId);

        return product.Stock.Value;
    }

    private async Task AddAvailableCount(
        int productId,
        int storageContentId,
        int count)
    {
        var storageContent = await Context.StorageContents.SingleAsync(x => x.Id == storageContentId);
        var product = await Context.Products.SingleAsync(x => x.Id == productId);

        storageContent.IncreaseCount(count);
        product.IncreaseStock(count);

        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }
}