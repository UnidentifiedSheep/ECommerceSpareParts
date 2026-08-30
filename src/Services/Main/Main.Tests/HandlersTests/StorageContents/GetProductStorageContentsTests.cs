using Abstractions.Models;
using Main.Application.Handlers.StorageContents.GetProductStorageContents;
using Main.Entities.Storage;
using Tests.DataBuilders.Storage;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.StorageContents;

public class GetProductStorageContentsTests : IntegrationTest
{
	public GetProductStorageContentsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<CurrencyRatesTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	[Fact]
	public async Task GetProductStorageContents_ForMultipleProducts_PaginatesEachProductIndependently()
	{
		var products = GetContext<ProductTestContext>().Products.Take(2).ToArray();
		var storage = GetContext<StorageTestContext>().Storages.First();
		var currency = GetContext<CurrencyRatesTestContext>().CurrencyTestContext.Currencies[0];
		var expected = new Dictionary<int, IReadOnlyList<int>>();

		foreach (var product in products)
		{
			var contents = await new StorageContentBuilder(Faker)
				.WithProductIds(product.Id)
				.WithStorageCode(storage.Code)
				.WithCurrencyId(currency.Id)
				.WithCount(1)
				.BuildManyAndAddToDb(Context, 5);
			expected[product.Id] = contents.OrderBy(x => x.Id).Skip(2).Take(2).Select(x => x.Id).ToArray();
		}

		var items = products
			.Select(product => new GetProductStorageContentsItem(
				product.Id,
				new Pagination(1, 2),
				storage.Code,
				false))
			.ToArray();

		var result = await Mediator.Send(new GetProductStorageContentsQuery(items));

		foreach (var item in items)
			Assert.Equal(expected[item.ProductId], result.Content[item].Select(x => x.Id));
	}

	[Fact]
	public async Task GetProductStorageContents_ForSameProduct_AppliesPaginationForEachItem()
	{
		var product = GetContext<ProductTestContext>().Products.First();
		var storage = GetContext<StorageTestContext>().Storages.First();
		var currency = GetContext<CurrencyRatesTestContext>().CurrencyTestContext.Currencies[0];
		var contents = await new StorageContentBuilder(Faker)
			.WithProductIds(product.Id)
			.WithStorageCode(storage.Code)
			.WithCurrencyId(currency.Id)
			.WithCount(1)
			.BuildManyAndAddToDb(Context, 5);
		var orderedIds = contents.OrderBy(x => x.Id).Select(x => x.Id).ToArray();
		var firstPage = new GetProductStorageContentsItem(
			product.Id,
			new Pagination(0, 2),
			storage.Code,
			false);
		var secondPage = firstPage with
		{
			Pagination = new Pagination(1, 2)
		};

		var result = await Mediator.Send(new GetProductStorageContentsQuery([firstPage, secondPage]));

		Assert.Equal(orderedIds.Take(2), result.Content[firstPage].Select(x => x.Id));
		Assert.Equal(orderedIds.Skip(2).Take(2), result.Content[secondPage].Select(x => x.Id));
	}

	[Fact]
	public async Task GetProductStorageContents_WithDifferentFilters_ReturnsContentForEachItem()
	{
		var product = GetContext<ProductTestContext>().Products.First();
		var storages = GetContext<StorageTestContext>().Storages.Take(2).ToArray();
		var currency = GetContext<CurrencyRatesTestContext>().CurrencyTestContext.Currencies[0];
		var positiveInFirstStorage = await CreateContent(
			product.Id,
			storages[0].Code,
			currency.Id,
			5);
		var zeroInFirstStorage = await CreateContent(
			product.Id,
			storages[0].Code,
			currency.Id,
			0);
		var positiveInSecondStorage = await CreateContent(
			product.Id,
			storages[1].Code,
			currency.Id,
			7);
		var pagination = new Pagination(0, 100);
		var positiveOnly = new GetProductStorageContentsItem(
			product.Id,
			pagination,
			storages[0].Code,
			false);
		var includingZero = positiveOnly with
		{
			ShowZeroCount = true
		};
		var allStorages = positiveOnly with
		{
			StorageCode = null
		};

		var result = await Mediator.Send(
			new GetProductStorageContentsQuery([positiveOnly, includingZero, allStorages]));

		Assert.Equal([positiveInFirstStorage.Id], result.Content[positiveOnly].Select(x => x.Id));
		Assert.Equal(
			new[]
			{
				positiveInFirstStorage.Id, zeroInFirstStorage.Id
			}.Order(),
			result.Content[includingZero].Select(x => x.Id));
		Assert.Equal(
			new[]
			{
				positiveInFirstStorage.Id, positiveInSecondStorage.Id
			}.Order(),
			result.Content[allStorages].Select(x => x.Id));
	}

	[Fact]
	public async Task GetProductStorageContents_WithUnknownProduct_ReturnsEmptyList()
	{
		var item = new GetProductStorageContentsItem(
			int.MaxValue,
			new Pagination(0, 10),
			null,
			true);

		var result = await Mediator.Send(new GetProductStorageContentsQuery([item]));

		Assert.Empty(result.Content[item]);
	}

	[Fact]
	public async Task GetProductStorageContents_WithDuplicateItems_ReturnsSingleEntry()
	{
		var productId = GetContext<ProductTestContext>().Products.First().Id;
		var item = new GetProductStorageContentsItem(
			productId,
			new Pagination(0, 10),
			null,
			true);

		var result = await Mediator.Send(new GetProductStorageContentsQuery([item, item]));

		Assert.Single(result.Content);
		Assert.True(result.Content.ContainsKey(item));
	}

	[Theory]
	[InlineData(-1, 10)]
	[InlineData(0, 0)]
	[InlineData(0, 101)]
	public async Task GetProductStorageContents_WithInvalidPagination_ThrowsValidationException(
		int page,
		int size)
	{
		var productId = GetContext<ProductTestContext>().Products.First().Id;
		var item = new GetProductStorageContentsItem(
			productId,
			new Pagination(page, size),
			null,
			true);

		await Assert.ThrowsAsync<ValidationException>(() =>
			Mediator.Send(new GetProductStorageContentsQuery([item])));
	}

	private async Task<StorageContent> CreateContent(
		int productId,
		string storageCode,
		int currencyId,
		int count)
	{
		return await new StorageContentBuilder(Faker)
			.WithProductIds(productId)
			.WithStorageCode(storageCode)
			.WithCurrencyId(currencyId)
			.WithCount(count)
			.BuildAndAddToDb(Context);
	}
}
