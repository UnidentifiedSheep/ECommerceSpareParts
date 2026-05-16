using FluentAssertions;
using Main.Application.Handlers.StorageContents.SubtractContent;
using Main.Entities.Event;
using Main.Entities.Exceptions.Storages;
using Main.Entities.Storage;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Test.Common.TestContainers.Combined;
using Tests.TestContexts.Storage;
using ValidationException = FluentValidation.ValidationException;

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
            new SubtractedStorageContent(passedContent.Id, passedContentOriginalCount),
            new SubtractedStorageContent(firstByDate.Id, 1));

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

        result.Contents.Should().Equal(new SubtractedStorageContent(content.Id, 2));

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

        result.Contents.Should().Equal(new SubtractedStorageContent(second.Id, 1));

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

    private IReadOnlyList<StorageContent> GetContentsForSameProductAndStorage(int minCount)
    {
        return GetContext<StorageContentTestContext>().StorageContents
            .GroupBy(x => new { x.ProductId, x.StorageName })
            .Where(x => x.Count() >= minCount)
            .OrderByDescending(x => x.Sum(z => z.Count))
            .First()
            .ToList();
    }
}