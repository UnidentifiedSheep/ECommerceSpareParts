using Tests.Abstractions.Test;
using FluentValidation.TestHelper;
using Abstractions.Models;
using Application.Common.Models;
using Application.Common.Querying;
using Exceptions;
using Main.Application.Handlers.StorageContents.GetStorageContents;
using Main.Entities.Storage;
using Tests.DataBuilders.Storage;
using Tests.Extensions;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.StorageContents;

public sealed class GetStorageContentsTests : IntegrationTest
{
	public GetStorageContentsTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<CurrencyRatesTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	[Fact]
	public async Task GetStorageContents_WithProductAndStorage_FiltersContent()
	{
		var products = GetContext<ProductTestContext>().Products.Take(2).ToArray();
		var storages = GetContext<StorageTestContext>().Storages.Take(2).ToArray();
		var currencyId = GetCurrencyId();
		var expected = await CreateContent(
			products[0].Id,
			storages[0].Code,
			currencyId,
			5);
		await CreateContent(
			products[0].Id,
			storages[0].Code,
			currencyId,
			0);
		await CreateContent(
			products[0].Id,
			storages[1].Code,
			currencyId,
			7);
		await CreateContent(
			products[1].Id,
			storages[0].Code,
			currencyId,
			9);

		var result = await Mediator.Send(
			new GetStorageContentsQuery(
				products[0].Id,
				storages[0].Code,
				[],
				new Pagination(0, 20),
				false),
			CancellationToken);

		Assert.Equal([expected.Id], result.Content.Select(x => x.Id));
	}

	[Fact]
	public async Task GetStorageContents_WithSortAndPagination_ReturnsRequestedPage()
	{
		var productId = GetContext<ProductTestContext>().Products.First().Id;
		var storageCode = GetContext<StorageTestContext>().Storages.First().Code;
		var currencyId = GetCurrencyId();
		await CreateContent(
			productId,
			storageCode,
			currencyId,
			10);
		var highest = await CreateContent(
			productId,
			storageCode,
			currencyId,
			30);
		var middle = await CreateContent(
			productId,
			storageCode,
			currencyId,
			20);

		var firstPage = await Mediator.Send(
			new GetStorageContentsQuery(
				productId,
				storageCode,
				["count_desc"],
				new Pagination(0, 2),
				true),
			CancellationToken);
		var secondPage = await Mediator.Send(
			new GetStorageContentsQuery(
				productId,
				storageCode,
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
		var productId = GetContext<ProductTestContext>().Products.First().Id;
		var storageCode = GetContext<StorageTestContext>().Storages.First().Code;
		var zeroContent = await CreateContent(
			productId,
			storageCode,
			GetCurrencyId(),
			0);

		var result = await Mediator.Send(
			new GetStorageContentsQuery(
				productId,
				storageCode,
				[],
				new Pagination(0, 20),
				true),
			CancellationToken);

		Assert.Contains(result.Content, x => x.Id == zeroContent.Id && x.Count == 0);
	}

	[Theory]
	[InlineData("unknown", null)]
	[InlineData("count", "unexpected")]
	public async Task GetStorageContents_WithInvalidSort_ThrowsInvalidInputException(string field, string? argument)
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
		var product = GetContext<ProductTestContext>().Products[0];
		var storage = GetContext<StorageTestContext>().Storages.First();
		var first = await CreateContent(product.Id, storage.Code, GetCurrencyId(), 10);
		var second = await CreateContent(product.Id, storage.Code, GetCurrencyId(), 5);
		var third = await CreateContent(product.Id, storage.Code, GetCurrencyId(), 8);
		var overlocked = await CreateContent(product.Id, storage.Code, GetCurrencyId(), 1);
		var now = DateTimeOffset.UtcNow;
		var released = StorageContentLock.Create(second.Id, 100);
		released.Release(now.UtcDateTime);
		Context.StorageContentLocks.AddRange(
			StorageContentLock.Create(first.Id, 8, now.AddHours(1).UtcDateTime),
			StorageContentLock.Create(third.Id, 3),
			StorageContentLock.Create(overlocked.Id, 4), released);
		await Context.SaveChangesAsync(CancellationToken);
		var sort = new SortExpression("availableCount");

		var result = await Mediator.Send(new GetStorageContentsQuery(
			product.Id, storage.Code, [sort], new Pagination(0, 100), true), CancellationToken);

		Assert.Equal(new[] { overlocked.Id, first.Id, second.Id, third.Id }, result.Content.Select(content => content.Id));
	}

	private int GetCurrencyId() =>
		GetContext<CurrencyRatesTestContext>().CurrencyTestContext.Currencies[0].Id;

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

public class GetStorageContentsValidationTests : ValidationTest<GetStorageContentsValidation, GetStorageContentsQuery>
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
