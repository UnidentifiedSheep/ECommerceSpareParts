using Tests.Extensions;
using FluentAssertions;
using Application.Common.Interfaces.Persistence;
using Attributes;
using Main.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Main.Application.Models.Storage;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Enums;
using Microsoft.EntityFrameworkCore;
using Tests.TestContainers.Combined;
using Tests.TestContexts.Storage;

namespace Tests.ServicesTests;

public class StorageContentServiceByContentsTests : IntegrationTest
{
	public StorageContentServiceByContentsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<StorageContentTestContext>();
	}

	[Fact]
	public async Task ByContents_StartsFromPassedStorageContentAndContinuesOnSameStorage()
	{
		var contentsForSameProductAndStorage = GetContentsForSameProductAndStorage(2)
			.OrderBy(x => x.PurchaseDatetime)
			.ToList();
		var firstByDate = contentsForSameProductAndStorage.First();
		var passedContent = contentsForSameProductAndStorage.Last();
		var product = await Context.Products.SingleAsync(
			x => x.Id == passedContent.ProductId,
			CancellationToken);
		var productStockQuantity = await Context.GetProductStockQuantityAsync(product.Id, CancellationToken);
		var originalStock = productStockQuantity;
		var passedContentOriginalCount = passedContent.Count;
		var firstByDateOriginalCount = firstByDate.Count;
		var countToSubtract = passedContent.Count + 1;

		var result = await Withdraw(
			passedContent.Id,
			countToSubtract,
			StorageMovementType.Sale);

		result
			.Should()
			.Equal(ToStorageLot(passedContent, passedContentOriginalCount), ToStorageLot(firstByDate, 1));

		var contents =
			await Context.StorageContents.AsNoTracking().ToDictionaryAsync(x => x.Id, CancellationToken);
		contents[passedContent.Id].Count.Should().Be(0);
		contents[firstByDate.Id].Count.Should().Be(firstByDateOriginalCount - 1);

		var updatedProductStockQuantity = await Context.GetProductStockQuantityAsync(product.Id, CancellationToken);
		updatedProductStockQuantity.Should().Be(originalStock - countToSubtract);
	}

	[Fact]
	public async Task ByContents_WhenFirstStorageContentHasEnoughCount_UsesOnlyIt()
	{
		var content = GetContext<StorageContentTestContext>().StorageContents.First(x => x.Count >= 2);
		var originalCount = content.Count;
		var productStockQuantity = await Context.GetProductStockQuantityAsync(content.ProductId, CancellationToken);

		var result = await Withdraw(
			content.Id,
			2,
			StorageMovementType.Sale);

		result.Should().Equal(ToStorageLot(content, 2));

		var updatedContent = await Context
			.StorageContents
			.AsNoTracking()
			.SingleAsync(x => x.Id == content.Id, CancellationToken);
		var updatedProductStockQuantity = await Context.GetProductStockQuantityAsync(content.ProductId, CancellationToken);
		updatedContent.Count.Should().Be(originalCount - 2);
		updatedProductStockQuantity.Should().Be(productStockQuantity - 2);
	}

	[Fact]
	public async Task ByContents_WhenFirstStorageContentIsZero_ContinuesOnSameStorage()
	{
		var contentsForSameProductAndStorage = GetContentsForSameProductAndStorage(2)
			.OrderBy(x => x.PurchaseDatetime)
			.ToList();
		var first = contentsForSameProductAndStorage.First();
		var second = contentsForSameProductAndStorage.Skip(1).First();
		var productStockQuantity = await Context.GetProductStockQuantityAsync(first.ProductId, CancellationToken);
		var originalFirstCount = first.Count;
		var originalSecondCount = second.Count;
		first.SetCount(0, StorageMovementType.StorageContentEditing);
		var stock = await Context.ProductStocks.SingleAsync(
			item => item.ProductId == first.ProductId && item.StorageCode == first.StorageCode, CancellationToken);
		stock.SetQuantity(stock.Quantity - originalFirstCount);
		await Context.SaveChangesAsync(CancellationToken);

		var result = await Withdraw(
			first.Id,
			1,
			StorageMovementType.Sale);

		result.Should().Equal(ToStorageLot(second, 1));

		var updatedFirst = await Context
			.StorageContents
			.AsNoTracking()
			.SingleAsync(x => x.Id == first.Id, CancellationToken);
		var updatedSecond = await Context
			.StorageContents
			.AsNoTracking()
			.SingleAsync(x => x.Id == second.Id, CancellationToken);
		updatedFirst.Count.Should().Be(0);
		updatedSecond.Count.Should().Be(originalSecondCount - 1);
	}

	[Fact]
	public async Task ByContents_WhenCountEqualsAvailableOnSameStorage_ConsumesAll()
	{
		var content = GetContext<StorageContentTestContext>().StorageContents.First();
		var countToSubtract = await Context
			.StorageContents
			.AsNoTracking()
			.Where(x => x.ProductId == content.ProductId && x.StorageCode == content.StorageCode)
			.SumAsync(x => x.Count, CancellationToken);

		var result = await Withdraw(
			content.Id,
			countToSubtract,
			StorageMovementType.Sale);

		result.Sum(x => x.Count).Should().Be(countToSubtract);

		var leftOnSameStorage = await Context
			.StorageContents
			.AsNoTracking()
			.Where(x => x.ProductId == content.ProductId && x.StorageCode == content.StorageCode)
			.SumAsync(x => x.Count, CancellationToken);
		leftOnSameStorage.Should().Be(0);
	}

	[Fact]
	public async Task ByContents_WithBatch_SubtractsEveryItem()
	{
		var contentsForSameProductAndStorage = GetContentsForSameProductAndStorage(2)
			.OrderBy(x => x.PurchaseDatetime)
			.ToList();
		var first = contentsForSameProductAndStorage.First();
		var second = contentsForSameProductAndStorage.Skip(1).First();
		var productStockQuantity = await Context.GetProductStockQuantityAsync(first.ProductId, CancellationToken);
		var originalFirstCount = first.Count;
		var originalSecondCount = second.Count;

		var result = await Withdraw(
			[new SubtractStorageContentItem(first.Id, 1), new SubtractStorageContentItem(second.Id, 1)],
			StorageMovementType.PurchaseEditing);

		result.Should().Equal(ToStorageLot(first, 1), ToStorageLot(second, 1));

		var contents =
			await Context.StorageContents.AsNoTracking().ToDictionaryAsync(x => x.Id, CancellationToken);
		contents[first.Id].Count.Should().Be(originalFirstCount - 1);
		contents[second.Id].Count.Should().Be(originalSecondCount - 1);

		var updatedProductStockQuantity = await Context.GetProductStockQuantityAsync(first.ProductId, CancellationToken);
		updatedProductStockQuantity.Should().Be(productStockQuantity - 2);
	}

	[Fact]
	public async Task
		ByContents_WithBatch_WhenOneItemHasNotEnoughCount_DoesNotPersistPartialChanges()
	{
		var contentsForSameProductAndStorage = GetContentsForSameProductAndStorage(2)
			.OrderBy(x => x.PurchaseDatetime)
			.ToList();
		var first = contentsForSameProductAndStorage.First();
		var second = contentsForSameProductAndStorage.Skip(1).First();
		var originalCounts = await Context
			.StorageContents
			.AsNoTracking()
			.Where(x => x.ProductId == first.ProductId && x.StorageCode == first.StorageCode)
			.ToDictionaryAsync(
				x => x.Id,
				x => x.Count,
				CancellationToken);
		var originalProductStockQuantity = await Context.GetProductStockQuantityAsync(first.ProductId, CancellationToken);
		var countOnSameStorage = originalCounts.Values.Sum();

		await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() => Withdraw(
			[
				new SubtractStorageContentItem(first.Id, 1),
				new SubtractStorageContentItem(second.Id, countOnSameStorage + 1)
			],
			StorageMovementType.PurchaseEditing));

		var actualCounts = await Context
			.StorageContents
			.AsNoTracking()
			.Where(x => x.ProductId == first.ProductId && x.StorageCode == first.StorageCode)
			.ToDictionaryAsync(
				x => x.Id,
				x => x.Count,
				CancellationToken);
		var actualProductStockQuantity = await Context.GetProductStockQuantityAsync(first.ProductId, CancellationToken);
		actualCounts.Should().Equal(originalCounts);
		actualProductStockQuantity.Should().Be(originalProductStockQuantity);
	}

	[Fact]
	public async Task
		ByContents_WhenNotEnoughCountOnSameStorage_ThrowsNotEnoughCountOnStorageException()
	{
		var content = GetContext<StorageContentTestContext>().StorageContents.First();
		var countOnSameStorage = await Context
			.StorageContents
			.AsNoTracking()
			.Where(x => x.ProductId == content.ProductId && x.StorageCode == content.StorageCode)
			.SumAsync(x => x.Count, CancellationToken);

		await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() => Withdraw(
			content.Id,
			countOnSameStorage + 1,
			StorageMovementType.Sale));
	}

	[Fact]
	public async Task ByContents_WhenNotEnoughCount_DoesNotPersistPartialChanges()
	{
		var content = GetContext<StorageContentTestContext>().StorageContents.First();
		var originalCounts = await Context
			.StorageContents
			.AsNoTracking()
			.Where(x => x.ProductId == content.ProductId && x.StorageCode == content.StorageCode)
			.ToDictionaryAsync(
				x => x.Id,
				x => x.Count,
				CancellationToken);
		var originalProductStockQuantity = await Context.GetProductStockQuantityAsync(content.ProductId, CancellationToken);
		var countOnSameStorage = originalCounts.Values.Sum();

		await Assert.ThrowsAsync<NotEnoughCountOnStorageException>(() => Withdraw(
			content.Id,
			countOnSameStorage + 1,
			StorageMovementType.Sale));

		var actualCounts = await Context
			.StorageContents
			.AsNoTracking()
			.Where(x => x.ProductId == content.ProductId && x.StorageCode == content.StorageCode)
			.ToDictionaryAsync(
				x => x.Id,
				x => x.Count,
				CancellationToken);
		var actualProductStockQuantity = await Context.GetProductStockQuantityAsync(content.ProductId, CancellationToken);
		actualCounts.Should().Equal(originalCounts);
		actualProductStockQuantity.Should().Be(originalProductStockQuantity);
	}

	[Fact]
	public async Task
		ByContents_WithMissingStorageContent_ThrowsStorageContentNotFoundException()
	{
		await Assert.ThrowsAsync<StorageContentNotFoundException>(() => Withdraw(
			int.MaxValue,
			1,
			StorageMovementType.Sale));
	}

	[Fact]
	public async Task ByContents_WithEmptyBatch_ThrowsValidationException()
	{
		await Assert.ThrowsAsync<ValidationException>(() => Withdraw([], StorageMovementType.Sale));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task ByContents_WithInvalidStorageContentId_ThrowsValidationException(
		int storageContentId)
	{
		await Assert.ThrowsAsync<ValidationException>(() => Withdraw(
			storageContentId,
			1,
			StorageMovementType.Sale));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task ByContents_WithInvalidCount_ThrowsValidationException(int count)
	{
		await Assert.ThrowsAsync<ValidationException>(() => Withdraw(
			1,
			count,
			StorageMovementType.Sale));
	}

	private Task<IReadOnlyList<StorageLot>> Withdraw(int storageContentId, int count, StorageMovementType movementType) =>
		Withdraw([new SubtractStorageContentItem(storageContentId, count)], movementType);

	private Task<IReadOnlyList<StorageLot>> Withdraw(
		IReadOnlyCollection<SubtractStorageContentItem> items, StorageMovementType movementType) =>
		Scope.ServiceProvider.GetRequiredService<IApplicationTransaction>()
			.WithSettings(TransactionalAttribute.ReadCommitted(0, 0)).ExecuteAsync(async (context, token) =>
		{
			var result = await context.GetRequiredService<IStorageContentService>()
			.SubtractByContentsAsync(items, movementType, token);
			await context.UnitOfWork.SaveChangesAsync(token);
			return result;
		}, CancellationToken);

	private IReadOnlyList<StorageContent> GetContentsForSameProductAndStorage(int minCount)
	{
		return GetContext<StorageContentTestContext>()
			.StorageContents
			.Where(x => x.Count > 0)
			.GroupBy(x => new
			{
				x.ProductId, x.StorageCode
			})
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
