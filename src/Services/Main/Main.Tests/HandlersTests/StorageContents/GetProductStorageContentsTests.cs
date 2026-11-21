using Application.Common.Models;
using Main.Entities.Storage.Storage;
using Main.Modules.Storage.Application.Queries.StorageContents.GetProductStorageContents;
using Tests.Abstractions.Test;
using Tests.DataBuilders.Storage;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.StorageContents;

public class GetProductStorageContentsTests(
	ReadScenarioFixture<GetProductStorageContentsTests.ProductStorageContentsScenario> fixture)
	: ReadScenarioTest<GetProductStorageContentsTests.ProductStorageContentsScenario>(fixture)
{
	[Fact]
	public async Task GetProductStorageContents_ForMultipleProducts_PaginatesEachProductIndependently()
	{
		var storageCode = Scenario.Get<StorageTestContext>().Storages.First().Code;
		var items = Scenario.PagedContents
			.Keys
			.Select(productId => new GetProductStorageContentsItem(
				productId,
				new Pagination(1, 2),
				storageCode,
				false))
			.ToArray();

		var result = await Mediator.Send(new GetProductStorageContentsQuery(items), CancellationToken);

		foreach (var item in items)
			Assert.Equal(
				Scenario.PagedContents[item.ProductId].Skip(2).Take(2),
				result.Content[item].Select(x => x.Id));
	}

	[Fact]
	public async Task GetProductStorageContents_ForSameProduct_AppliesPaginationForEachItem()
	{
		var (productId, orderedIds) = Scenario.PagedContents.First();
		var firstPage = new GetProductStorageContentsItem(
			productId,
			new Pagination(0, 2),
			Scenario.Get<StorageTestContext>().Storages.First().Code,
			false);
		var secondPage = firstPage with
		{
			Pagination = new Pagination(1, 2)
		};

		var result = await Mediator.Send(
			new GetProductStorageContentsQuery([firstPage, secondPage]),
			CancellationToken);

		Assert.Equal(orderedIds.Take(2), result.Content[firstPage].Select(x => x.Id));
		Assert.Equal(orderedIds.Skip(2).Take(2), result.Content[secondPage].Select(x => x.Id));
	}

	[Fact]
	public async Task GetProductStorageContents_WithDifferentFilters_ReturnsContentForEachItem()
	{
		var positiveInFirstStorage = Scenario.PositiveInFirstStorage;
		var pagination = new Pagination(0, 100);
		var positiveOnly = new GetProductStorageContentsItem(
			positiveInFirstStorage.ProductId,
			pagination,
			positiveInFirstStorage.StorageCode,
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
			new GetProductStorageContentsQuery([positiveOnly, includingZero, allStorages]),
			CancellationToken);

		Assert.Equal([positiveInFirstStorage.Id], result.Content[positiveOnly].Select(x => x.Id));
		Assert.Equal(
			new[]
			{
				positiveInFirstStorage.Id, Scenario.ZeroInFirstStorage.Id
			}.Order(),
			result.Content[includingZero].Select(x => x.Id));
		Assert.Equal(
			new[]
			{
				positiveInFirstStorage.Id, Scenario.PositiveInSecondStorage.Id
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

		var result = await Mediator.Send(new GetProductStorageContentsQuery([item]), CancellationToken);

		Assert.Empty(result.Content[item]);
	}

	[Fact]
	public async Task GetProductStorageContents_WithDuplicateItems_ReturnsSingleEntry()
	{
		var productId = Scenario.Get<ProductTestContext>().Products.First().Id;
		var item = new GetProductStorageContentsItem(
			productId,
			new Pagination(0, 10),
			null,
			true);

		var result = await Mediator.Send(new GetProductStorageContentsQuery([item, item]), CancellationToken);

		Assert.Single(result.Content);
		Assert.True(result.Content.ContainsKey(item));
	}

	public sealed class ProductStorageContentsScenario : ReadScenario
	{
		private readonly Dictionary<int, StorageContent[]> _pagedContents = [];

		public ProductStorageContentsScenario()
		{
			RegisterBasicContext<ProductTestContext>();
			RegisterBasicContext<CurrencyRatesTestContext>();
			RegisterBasicContext<StorageTestContext>();
		}

		public IReadOnlyDictionary<int, IReadOnlyList<int>> PagedContents
			=> _pagedContents.ToDictionary(
				x => x.Key,
				IReadOnlyList<int> (x) => x.Value.Select(content => content.Id).Order().ToArray());

		public StorageContent PositiveInFirstStorage { get; private set; } = null!;

		public StorageContent ZeroInFirstStorage { get; private set; } = null!;

		public StorageContent PositiveInSecondStorage { get; private set; } = null!;

		protected override IEnumerable<object> Seed()
		{
			var products = Get<ProductTestContext>().Products;
			var storages = Get<StorageTestContext>().Storages.Select(x => x.Code).ToArray();
			foreach (var product in products.Take(2))
				_pagedContents[product.Id] = Enumerable.Range(0, 5)
					.Select(_ => CreateContent(product.Id, storages[0], 1))
					.ToArray();
			PositiveInFirstStorage = CreateContent(products[2].Id, storages[0], 5);
			ZeroInFirstStorage = CreateContent(products[2].Id, storages[0], 0);
			PositiveInSecondStorage = CreateContent(products[2].Id, storages[1], 7);
			return
			[
				.._pagedContents.Values.SelectMany(x => x),
				PositiveInFirstStorage,
				ZeroInFirstStorage,
				PositiveInSecondStorage
			];
		}

		private StorageContent CreateContent(int productId, string storageCode, int count)
			=> new StorageContentBuilder(Faker)
				.WithProductIds(productId)
				.WithStorageCode(storageCode)
				.WithCurrencyId(Get<CurrencyRatesTestContext>().CurrencyTestContext.Currencies[0].Id)
				.WithCount(count)
				.Build();
	}
}

public class GetProductStorageContentsValidationTests : ValidationTest<GetProductStorageContentsValidation,
	GetProductStorageContentsQuery>
{
	[Theory]
	[InlineData(-1, 10)]
	[InlineData(0, 0)]
	[InlineData(0, 101)]
	public async Task GetProductStorageContents_WithInvalidPagination_FailsValidation(
		int page,
		int size)
	{
		var productId = 1;
		var item = new GetProductStorageContentsItem(
			productId,
			new Pagination(page, size),
			null,
			true);

		var result = await ValidateAsync(
			new GetProductStorageContentsQuery([item]));
		result.ShouldHaveValidationErrorFor(
			page < 0 ? "Items[0].Pagination.Page" : "Items[0].Pagination.Size");
	}
}
