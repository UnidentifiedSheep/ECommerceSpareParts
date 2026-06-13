using FluentAssertions;
using Main.Application.Handlers.StorageContents.SubtractContent;
using Main.Application.Models.Storage;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.StorageContents;

public class SubtractStorageContentsTests : IntegrationTest
{
    public SubtractStorageContentsTests(CombinedContainerFixture fixture) : base(fixture)
    {
        RegisterBasicContext<StorageContentTestContext>();
    }

    [Fact]
    public async Task SubtractStorageContents_StartsFromPassedStorageContentAndContinuesOnSameStorage()
    {
        var contentsForSameProductAndStorage = GetContentsForSameProductAndStorage(2)
            .OrderBy(x => x.PurchaseDatetime)
            .ToList();
        var firstByDate = contentsForSameProductAndStorage.First();
        var passedContent = contentsForSameProductAndStorage.Last();
        var product = await Context.Products.SingleAsync(x => x.Id == passedContent.ProductId);
        var originalStock = product.Stock.Value;
        var passedContentOriginalCount = passedContent.Count;
        var firstByDateOriginalCount = firstByDate.Count;
        var countToSubtract = passedContent.Count + 1;

        var result = await Mediator.Send(new SubtractStorageContentsCommand(
            passedContent.Id,
            countToSubtract,
            StorageMovementType.Sale));

        result.Contents.Should().Equal(
            ToStorageLot(passedContent, passedContentOriginalCount),
            ToStorageLot(firstByDate, 1));

        var contents = await Context.StorageContents.AsNoTracking().ToDictionaryAsync(x => x.Id);
        contents[passedContent.Id].Count.Should().Be(0);
        contents[firstByDate.Id].Count.Should().Be(firstByDateOriginalCount - 1);

        var updatedProduct = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == product.Id);
        updatedProduct.Stock.Value.Should().Be(originalStock - countToSubtract);

        var events = await Context.Events.OfType<StorageMovementEvent>().AsNoTracking().ToListAsync();
        events.Should().HaveCount(2);
        events.Should().Contain(x =>
            x.Data.ProductId == product.Id &&
            x.Data.StorageName == passedContent.StorageName &&
            x.Data.Count == passedContentOriginalCount &&
            x.Data.MovementType == StorageMovementType.Sale);
        events.Should().Contain(x =>
            x.Data.ProductId == product.Id &&
            x.Data.StorageName == passedContent.StorageName &&
            x.Data.Count == 1 &&
            x.Data.MovementType == StorageMovementType.Sale);
    }

    [Fact]
    public async Task SubtractStorageContents_WhenFirstStorageContentHasEnoughCount_UsesOnlyIt()
    {
        var content = GetContext<StorageContentTestContext>().StorageContents
            .First(x => x.Count >= 2);
        var originalCount = content.Count;
        var product = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == content.ProductId);

        var result = await Mediator.Send(new SubtractStorageContentsCommand(
            content.Id,
            2,
            StorageMovementType.Sale));

        result.Contents.Should().Equal(ToStorageLot(content, 2));

        var updatedContent = await Context.StorageContents.AsNoTracking().SingleAsync(x => x.Id == content.Id);
        var updatedProduct = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == content.ProductId);
        updatedContent.Count.Should().Be(originalCount - 2);
        updatedProduct.Stock.Value.Should().Be(product.Stock.Value - 2);
    }

    [Fact]
    public async Task SubtractStorageContents_WhenFirstStorageContentIsZero_ContinuesOnSameStorage()
    {
        var contentsForSameProductAndStorage = GetContentsForSameProductAndStorage(2)
            .OrderBy(x => x.PurchaseDatetime)
            .ToList();
        var first = contentsForSameProductAndStorage.First();
        var second = contentsForSameProductAndStorage.Skip(1).First();
        var product = await Context.Products.SingleAsync(x => x.Id == first.ProductId);
        var originalFirstCount = first.Count;
        var originalSecondCount = second.Count;
        first.SetCount(0);
        product.IncreaseStock(-originalFirstCount);
        await Context.SaveChangesAsync();

        var result = await Mediator.Send(new SubtractStorageContentsCommand(
            first.Id,
            1,
            StorageMovementType.Sale));

        result.Contents.Should().Equal(ToStorageLot(second, 1));

        var updatedFirst = await Context.StorageContents.AsNoTracking().SingleAsync(x => x.Id == first.Id);
        var updatedSecond = await Context.StorageContents.AsNoTracking().SingleAsync(x => x.Id == second.Id);
        updatedFirst.Count.Should().Be(0);
        updatedSecond.Count.Should().Be(originalSecondCount - 1);
    }

    [Fact]
    public async Task SubtractStorageContents_WhenCountEqualsAvailableOnSameStorage_ConsumesAll()
    {
        var content = GetContext<StorageContentTestContext>().StorageContents.First();
        var countToSubtract = await Context.StorageContents
            .AsNoTracking()
            .Where(x => x.ProductId == content.ProductId && x.StorageName == content.StorageName)
            .SumAsync(x => x.Count);

        var result = await Mediator.Send(new SubtractStorageContentsCommand(
            content.Id,
            countToSubtract,
            StorageMovementType.Sale));

        result.Contents.Sum(x => x.Count).Should().Be(countToSubtract);

        var leftOnSameStorage = await Context.StorageContents
            .AsNoTracking()
            .Where(x => x.ProductId == content.ProductId && x.StorageName == content.StorageName)
            .SumAsync(x => x.Count);
        leftOnSameStorage.Should().Be(0);
    }

    [Fact]
    public async Task SubtractStorageContents_WithBatch_SubtractsEveryItem()
    {
        var contentsForSameProductAndStorage = GetContentsForSameProductAndStorage(2)
            .OrderBy(x => x.PurchaseDatetime)
            .ToList();
        var first = contentsForSameProductAndStorage.First();
        var second = contentsForSameProductAndStorage.Skip(1).First();
        var product = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == first.ProductId);
        var originalFirstCount = first.Count;
        var originalSecondCount = second.Count;

        var result = await Mediator.Send(new SubtractStorageContentsCommand(
            [
                new SubtractStorageContentItem(first.Id, 1),
                new SubtractStorageContentItem(second.Id, 1)
            ],
            StorageMovementType.PurchaseEditing));

        result.Contents.Should().Equal(
            ToStorageLot(first, 1),
            ToStorageLot(second, 1));

        var contents = await Context.StorageContents.AsNoTracking().ToDictionaryAsync(x => x.Id);
        contents[first.Id].Count.Should().Be(originalFirstCount - 1);
        contents[second.Id].Count.Should().Be(originalSecondCount - 1);

        var updatedProduct = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == first.ProductId);
        updatedProduct.Stock.Value.Should().Be(product.Stock.Value - 2);

        var events = await Context.Events.OfType<StorageMovementEvent>().AsNoTracking().ToListAsync();
        events.Should().HaveCount(2);
        events.Should().OnlyContain(x =>
            x.Data.ProductId == first.ProductId &&
            x.Data.StorageName == first.StorageName &&
            x.Data.Count == 1 &&
            x.Data.MovementType == StorageMovementType.PurchaseEditing);
    }

    [Fact]
    public async Task SubtractStorageContents_WithBatch_WhenOneItemHasNotEnoughCount_DoesNotPersistPartialChanges()
    {
        var contentsForSameProductAndStorage = GetContentsForSameProductAndStorage(2)
            .OrderBy(x => x.PurchaseDatetime)
            .ToList();
        var first = contentsForSameProductAndStorage.First();
        var second = contentsForSameProductAndStorage.Skip(1).First();
        var originalCounts = await Context.StorageContents
            .AsNoTracking()
            .Where(x => x.ProductId == first.ProductId && x.StorageName == first.StorageName)
            .ToDictionaryAsync(x => x.Id, x => x.Count);
        var originalProduct = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == first.ProductId);
        var countOnSameStorage = originalCounts.Values.Sum();

        await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() =>
            Mediator.Send(new SubtractStorageContentsCommand(
                [
                    new SubtractStorageContentItem(first.Id, 1),
                    new SubtractStorageContentItem(second.Id, countOnSameStorage + 1)
                ],
                StorageMovementType.PurchaseEditing)));

        var actualCounts = await Context.StorageContents
            .AsNoTracking()
            .Where(x => x.ProductId == first.ProductId && x.StorageName == first.StorageName)
            .ToDictionaryAsync(x => x.Id, x => x.Count);
        var actualProduct = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == first.ProductId);
        actualCounts.Should().Equal(originalCounts);
        actualProduct.Stock.Value.Should().Be(originalProduct.Stock.Value);

        var events = await Context.Events.OfType<StorageMovementEvent>().AsNoTracking().ToListAsync();
        events.Should().BeEmpty();
    }

    [Fact]
    public async Task SubtractStorageContents_WhenNotEnoughCountOnSameStorage_ThrowsNotEnoughCountOnStorageException()
    {
        var content = GetContext<StorageContentTestContext>().StorageContents.First();
        var countOnSameStorage = await Context.StorageContents
            .AsNoTracking()
            .Where(x => x.ProductId == content.ProductId && x.StorageName == content.StorageName)
            .SumAsync(x => x.Count);

        await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() =>
            Mediator.Send(new SubtractStorageContentsCommand(
                content.Id,
                countOnSameStorage + 1,
                StorageMovementType.Sale)));
    }

    [Fact]
    public async Task SubtractStorageContents_WhenNotEnoughCount_DoesNotPersistPartialChanges()
    {
        var content = GetContext<StorageContentTestContext>().StorageContents.First();
        var originalCounts = await Context.StorageContents
            .AsNoTracking()
            .Where(x => x.ProductId == content.ProductId && x.StorageName == content.StorageName)
            .ToDictionaryAsync(x => x.Id, x => x.Count);
        var originalProduct = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == content.ProductId);
        var countOnSameStorage = originalCounts.Values.Sum();

        await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() =>
            Mediator.Send(new SubtractStorageContentsCommand(
                content.Id,
                countOnSameStorage + 1,
                StorageMovementType.Sale)));

        var actualCounts = await Context.StorageContents
            .AsNoTracking()
            .Where(x => x.ProductId == content.ProductId && x.StorageName == content.StorageName)
            .ToDictionaryAsync(x => x.Id, x => x.Count);
        var actualProduct = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == content.ProductId);
        actualCounts.Should().Equal(originalCounts);
        actualProduct.Stock.Value.Should().Be(originalProduct.Stock.Value);
    }

    [Fact]
    public async Task SubtractStorageContents_WithMissingStorageContent_ThrowsStorageContentNotFoundException()
    {
        await Assert.ThrowsAsync<StorageContentNotFoundException>(() =>
            Mediator.Send(new SubtractStorageContentsCommand(
                int.MaxValue,
                1,
                StorageMovementType.Sale)));
    }

    [Fact]
    public async Task SubtractStorageContents_WithEmptyBatch_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(new SubtractStorageContentsCommand(
                [],
                StorageMovementType.Sale)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SubtractStorageContents_WithInvalidStorageContentId_ThrowsValidationException(
        int storageContentId)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(new SubtractStorageContentsCommand(
                storageContentId,
                1,
                StorageMovementType.Sale)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SubtractStorageContents_WithInvalidCount_ThrowsValidationException(int count)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(new SubtractStorageContentsCommand(
                1,
                count,
                StorageMovementType.Sale)));
    }

    [Fact]
    public async Task SubtractStorageContents_ByProductAndStorage_StartsFromPolicyContent()
    {
        var contentsForSameProductAndStorage = GetContentsForSameProductAndStorage(2)
            .OrderBy(x => x.PurchaseDatetime)
            .ToList();
        var firstByDate = contentsForSameProductAndStorage.First();
        var secondByDate = contentsForSameProductAndStorage.Skip(1).First();
        var product = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == firstByDate.ProductId);
        var firstByDateOriginalCount = firstByDate.Count;
        var secondByDateOriginalCount = secondByDate.Count;
        var countToSubtract = firstByDate.Count + 1;

        var result = await Mediator.Send(new SubtractStorageContentsCommand(
            [
                new SubtractProductFromStorageItem(
                    firstByDate.ProductId,
                    firstByDate.StorageName,
                    countToSubtract)
            ],
            StorageMovementType.Sale));

        result.Contents.Should().Equal(
            ToStorageLot(firstByDate, firstByDateOriginalCount),
            ToStorageLot(secondByDate, 1));

        var contents = await Context.StorageContents.AsNoTracking().ToDictionaryAsync(x => x.Id);
        contents[firstByDate.Id].Count.Should().Be(0);
        contents[secondByDate.Id].Count.Should().Be(secondByDateOriginalCount - 1);

        var updatedProduct = await Context.Products.AsNoTracking().SingleAsync(x => x.Id == product.Id);
        updatedProduct.Stock.Value.Should().Be(product.Stock.Value - countToSubtract);
    }

    [Fact]
    public async Task SubtractStorageContents_WithMixedItems_SubtractsEveryItem()
    {
        var groups = GetContext<StorageContentTestContext>().StorageContents
            .Where(x => x.Count > 0)
            .GroupBy(x => new { x.ProductId, x.StorageName })
            .Where(x => x.Count() >= 1)
            .Take(2)
            .Select(x => x.First())
            .ToList();

        groups.Should().HaveCount(2);

        var result = await Mediator.Send(new SubtractStorageContentsCommand(
            [
                new SubtractStorageContentItem(groups[0].Id, 1),
                new SubtractProductFromStorageItem(groups[1].ProductId, groups[1].StorageName, 1)
            ],
            StorageMovementType.Sale));

        result.Contents.Should().HaveCount(2);
        result.Contents.Should().OnlyContain(x => x.Count == 1);
    }

    [Fact]
    public async Task SubtractStorageContents_ByProductAndStorage_WhenNoContentOnStorage_ThrowsNotEnoughCount()
    {
        var product = await Context.Products.AsNoTracking().FirstAsync();

        await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() =>
            Mediator.Send(new SubtractStorageContentsCommand(
                [
                    new SubtractProductFromStorageItem(
                        product.Id,
                        "missing-storage",
                        1)
                ],
                StorageMovementType.Sale)));
    }

    [Fact]
    public async Task SubtractStorageContents_ByProductAndStorage_WithInvalidProductId_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(new SubtractStorageContentsCommand(
                [
                    new SubtractProductFromStorageItem(
                        0,
                        "storage",
                        1)
                ],
                StorageMovementType.Sale)));
    }

    [Fact]
    public async Task SubtractStorageContents_ByProductAndStorage_WithInvalidStorageName_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            Mediator.Send(new SubtractStorageContentsCommand(
                [
                    new SubtractProductFromStorageItem(
                        1,
                        "",
                        1)
                ],
                StorageMovementType.Sale)));
    }

    private IReadOnlyList<StorageContent> GetContentsForSameProductAndStorage(int minCount)
    {
        return GetContext<StorageContentTestContext>().StorageContents
            .Where(x => x.Count > 0)
            .GroupBy(x => new { x.ProductId, x.StorageName })
            .Where(x => x.Count() >= minCount)
            .OrderByDescending(x => x.Sum(z => z.Count))
            .First()
            .ToList();
    }

    private static StorageLot ToStorageLot(StorageContent content, int count)
    {
        return new StorageLot(
            content.Id,
            content.ProductId,
            content.CurrencyId,
            content.BuyPrice,
            count,
            content.PurchaseDatetime);
    }
}
