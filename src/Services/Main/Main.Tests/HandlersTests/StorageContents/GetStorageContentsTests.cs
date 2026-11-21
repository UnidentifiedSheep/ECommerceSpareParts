using Application.Common.Models;
using Application.Common.Querying;
using Exceptions;
using Main.Entities.Storage.Storage;
using Main.Modules.Storage.Application.Queries.StorageContents.GetStorageContents;
using Tests.Abstractions.Test;
using Tests.DataBuilders.Storage;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.StorageContents;

public sealed class GetStorageContentsTests(ReadScenarioFixture<GetStorageContentsTests.StorageContentsScenario> fixture)
	: ReadScenarioTest<GetStorageContentsTests.StorageContentsScenario>(fixture)
{
	[Fact]
	public async Task GetStorageContents_WithProductAndStorage_FiltersContent()
	{
		var expected = Scenario.FilteredContent;

		var result = await Mediator.Send(
			new GetStorageContentsQuery(
				expected.ProductId,
				expected.StorageCode,
				[],
				new Pagination(0, 20),
				false),
			CancellationToken);

		Assert.Equal([expected.Id], result.Content.Select(x => x.Id));
	}

	[Fact]
	public async Task GetStorageContents_WithSortAndPagination_ReturnsRequestedPage()
	{
		var highest = Scenario.HighestContent;
		var middle = Scenario.MiddleContent;

		var firstPage = await Mediator.Send(
			new GetStorageContentsQuery(
				highest.ProductId,
				highest.StorageCode,
				["count_desc"],
				new Pagination(0, 2),
				true),
			CancellationToken);
		var secondPage = await Mediator.Send(
			new GetStorageContentsQuery(
				highest.ProductId,
				highest.StorageCode,
				["count_desc"],
				new Pagination(1, 2),
				true),
			CancellationToken);

		Assert.Equal([highest.Id, middle.Id], firstPage.Content.Select(x => x.Id));
		Assert.Single(secondPage.Content);
		Assert.Equal(10, secondPage.Content[0].Count);
	}

	[Fact]
	public async Task GetStorageContents_WithShowZeroCount_ReturnsZeroContent()
	{
		var zeroContent = Scenario.ZeroContent;

		var result = await Mediator.Send(
			new GetStorageContentsQuery(
				zeroContent.ProductId,
				zeroContent.StorageCode,
				[],
				new Pagination(0, 20),
				true),
			CancellationToken);

		Assert.Contains(result.Content, x => x.Id == zeroContent.Id && x.Count == 0);
	}

	[Theory]
	[InlineData("unknown", null)]
	[InlineData("count", "unexpected")]
	public async Task GetStorageContents_WithInvalidSort_ThrowsInvalidInputException(
		string field,
		string? argument)
	{
		var query = new GetStorageContentsQuery(
			null,
			null,
			[new SortExpression(field, Argument: argument)],
			new Pagination(0, 20),
			true);

		await Assert.ThrowsAsync<InvalidInputException>(() => Mediator.Send(query, CancellationToken));
	}

	[Fact]
	public async Task AvailableCountSort_UsesActiveLocksClampsZeroAndBreaksTiesById()
	{
		var locked = Scenario.LockedContents;
		var sort = new SortExpression("availableCount");

		var result = await Mediator.Send(
			new GetStorageContentsQuery(
				locked[0].ProductId,
				locked[0].StorageCode,
				[sort],
				new Pagination(0, 100),
				true),
			CancellationToken);

		Assert.Equal(
			new[] { locked[3].Id, locked[0].Id, locked[1].Id, locked[2].Id },
			result.Content.Select(content => content.Id));
	}

	public sealed class StorageContentsScenario : ReadScenario
	{
		public StorageContentsScenario()
		{
			RegisterBasicContext<ProductTestContext>();
			RegisterBasicContext<CurrencyRatesTestContext>();
			RegisterBasicContext<StorageTestContext>();
		}

		public StorageContent FilteredContent { get; private set; } = null!;

		public StorageContent ZeroContent { get; private set; } = null!;

		public StorageContent HighestContent { get; private set; } = null!;

		public StorageContent MiddleContent { get; private set; } = null!;

		public IReadOnlyList<StorageContent> LockedContents { get; private set; } = null!;

		protected override async Task PrepareAsync()
		{
			var products = Get<ProductTestContext>().Products;
			var storages = Get<StorageTestContext>().Storages.Select(x => x.Code).ToArray();
			FilteredContent = CreateContent(products[0].Id, storages[0], 5);
			ZeroContent = CreateContent(products[0].Id, storages[0], 0);
			HighestContent = CreateContent(products[2].Id, storages[0], 30);
			MiddleContent = CreateContent(products[2].Id, storages[0], 20);
			LockedContents =
			[
				CreateContent(products[3].Id, storages[0], 10),
				CreateContent(products[3].Id, storages[0], 5),
				CreateContent(products[3].Id, storages[0], 8),
				CreateContent(products[3].Id, storages[0], 1)
			];
			Context.AddRange(
				[
					FilteredContent,
					ZeroContent,
					CreateContent(products[0].Id, storages[1], 7),
					CreateContent(products[1].Id, storages[0], 9),
					CreateContent(products[2].Id, storages[0], 10),
					HighestContent,
					MiddleContent,
					..LockedContents
				]);
			await Context.SaveChangesAsync(CancellationToken);
		}

		protected override IEnumerable<object> Seed()
		{
			var now = DateTime.UtcNow;
			var released = StorageContentLock.Create(LockedContents[1].Id, 100);
			released.Release(now);
			return
			[
				StorageContentLock.Create(LockedContents[0].Id, 8, now.AddHours(1)),
				StorageContentLock.Create(LockedContents[2].Id, 3),
				StorageContentLock.Create(LockedContents[3].Id, 4),
				released
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

public class
	GetStorageContentsValidationTests : ValidationTest<GetStorageContentsValidation, GetStorageContentsQuery>
{
	[Theory]
	[InlineData(-1, 20)]
	[InlineData(0, 0)]
	[InlineData(0, 101)]
	public async Task GetStorageContents_WithInvalidPagination_FailsValidation(int page, int size)
	{
		var query = new GetStorageContentsQuery(
			null,
			null,
			[],
			new Pagination(page, size),
			true);

		var result = await ValidateAsync(query);
		result.ShouldHaveValidationErrorFor(page < 0 ? "Pagination.Page" : "Pagination.Size");
	}
}
