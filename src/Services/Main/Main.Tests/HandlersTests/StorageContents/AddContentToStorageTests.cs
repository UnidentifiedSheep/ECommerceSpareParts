using Main.Application.Dtos.Storage;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Application.Static;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.DataBuilders.Storage;
using Tests.TestContainers.Combined;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;

namespace Tests.HandlersTests.StorageContents;

public class AddContentToStorageTests : IntegrationTest
{
	public AddContentToStorageTests(CombinedContainerFixture fixture) : base(fixture)
	{
		RegisterBasicContext<ProductTestContext>();
		RegisterBasicContext<CurrencyRatesTestContext>();
		RegisterBasicContext<StorageTestContext>();
	}

	[Fact]
	public async Task AddContentToStorage_WithEmptyContentList_ThrowsValidationException()
	{
		var storage = GetContext<StorageTestContext>().Storages.First();
		var command = new AddContentCommand(
			[],
			storage.Code,
			StorageMovementType.StorageContentAddition);
		await Assert.ThrowsAsync<ValidationException>(async () =>
			await Mediator.Send(command, CancellationToken));
	}

	[Theory]
	[InlineData(-1)]
	[InlineData(0.0001)]
	[InlineData(0.001)]
	[InlineData(0)]
	public async Task AddContentToStorage_WithInvalidPrice_ThrowsStorageContentPriceCannotBeNegativeException(
		decimal price)
	{
		var storage = GetContext<StorageTestContext>().Storages.First();
		var storageContent = GetNewStorageContents(3);
		storageContent[^1] = storageContent[^1] with
		{
			BuyPrice = price
		};
		var command = new AddContentCommand(
			storageContent,
			storage.Code,
			StorageMovementType.StorageContentAddition);
		await Assert.ThrowsAsync<ValidationException>(async () =>
			await Mediator.Send(command, CancellationToken));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public async Task
		AddContentToStorage_WithInvalidItemCount_ThrowsStorageContentCountCantBeNegativeException(int count)
	{
		var storage = GetContext<StorageTestContext>().Storages.First();
		var storageContent = GetNewStorageContents(3);
		storageContent[^1] = storageContent[^1] with
		{
			Count = count
		};
		var command = new AddContentCommand(
			storageContent,
			storage.Code,
			StorageMovementType.StorageContentAddition);
		await Assert.ThrowsAsync<ValidationException>(async () =>
			await Mediator.Send(command, CancellationToken));
	}

	[Fact]
	public async Task AddContentToStorage_WithInvalidCurrencyId_ThrowsCurrencyNotFoundException()
	{
		var storage = GetContext<StorageTestContext>().Storages.First();
		var storageContent = GetNewStorageContents(3);
		storageContent[^1] = storageContent[^1] with
		{
			CurrencyId = int.MaxValue
		};
		var command = new AddContentCommand(
			storageContent,
			storage.Code,
			StorageMovementType.StorageContentAddition);
		await Assert.ThrowsAsync<DbValidationException>(async () =>
			await Mediator.Send(command, CancellationToken));
	}

	[Fact]
	public async Task AddContentToStorage_WithInvalidStorageCode_ThrowsStorageNotFoundException()
	{
		var storageContent = GetNewStorageContents(3);
		var command = new AddContentCommand(
			storageContent,
			Faker.Lorem.Letter(200),
			StorageMovementType.StorageContentAddition);
		var exception =
			await Assert.ThrowsAsync<DbValidationException>(async () =>
				await Mediator.Send(command, CancellationToken));
		Assert.IsType<StorageNotFoundException>(exception.Failures[0].Error);
	}

	[Fact]
	public async Task AddContentToStorage_WithInvalidArticleId_ThrowsArticleNotFoundException()
	{
		var storage = GetContext<StorageTestContext>().Storages.First();
		var storageContent = GetNewStorageContents(3);
		storageContent[^1] = storageContent[^1] with
		{
			ProductId = int.MaxValue
		};
		var command = new AddContentCommand(
			storageContent,
			storage.Code,
			StorageMovementType.StorageContentAddition);
		var countBefore = await Context.StorageContents.CountAsync(CancellationToken);
		var exception = await Assert.ThrowsAsync<DbValidationException>(() => Mediator.Send(command, CancellationToken));
		Assert.Contains(exception.Failures, failure => failure.Error is ProductNotFoundException);
		Assert.Equal(countBefore, await Context.StorageContents.CountAsync(CancellationToken));
	}

	[Fact]
	public async Task AddContentToStorage_Normal_Succeeds()
	{
		var storage = GetContext<StorageTestContext>().Storages.First();
		var result = await RunSingleAddAsync(storage, Mediator);
		await AssertCorrectResult(storage, result);
	}

	[Fact]
	public async Task AddContentToStorage_ParallelExecution_Succeeds()
	{
		using var sc = Sp.CreateScope();
		var mediator = sc.ServiceProvider.GetRequiredService<IMediator>();
		var storage = GetContext<StorageTestContext>().Storages.First();
		var task1 = RunSingleAddAsync(storage, Mediator);
		var task2 = RunSingleAddAsync(storage, mediator);

		var results = await Task.WhenAll(task1, task2);

		var res1 = results[0];
		var res2 = results[1];

		await AssertCorrectResult(
			storage,
			res1,
			res2);
	}

	private async Task<(List<NewStorageContentDto> Inputs, Dictionary<int, int> TotalPerProduct)>
		RunSingleAddAsync(Storage storage, IMediator mediator)
	{
		var storageContent = GetNewStorageContents(3);

		var command = new AddContentCommand(
			storageContent,
			storage.Code,
			StorageMovementType.StorageContentAddition);
		await mediator.Send(command);

		var totalPerArticle = storageContent
			.GroupBy(x => x.ProductId)
			.ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

		return (storageContent, totalPerArticle);
	}

	private async Task AssertCorrectResult(
		Storage storage,
		params (List<NewStorageContentDto> Inputs, Dictionary<int, int> TotalPerProduct)[] results)
	{
		var allInputs = results.SelectMany(r => r.Inputs).ToList();

		var expectedTotals = new Dictionary<int, int>();
		foreach (var (_, totals) in results)
		foreach (var kv in totals)
			if (!expectedTotals.TryAdd(kv.Key, kv.Value))
				expectedTotals[kv.Key] += kv.Value;

		var stocks = await Context.ProductStocks.AsNoTracking().Where(stock => stock.StorageCode == storage.Code)
			.ToDictionaryAsync(stock => stock.ProductId, stock => stock.Quantity, CancellationToken);

		var dbStorageContents = await Context
			.StorageContents
			.AsNoTracking()
			.Where(x => x.StorageCode == storage.Code)
			.ToListAsync();

		foreach (var (productId, expectedTotal) in expectedTotals)
			Assert.Equal(expectedTotal, stocks[productId]);

		Assert.Equal(allInputs.Count, dbStorageContents.Count);

		var groupedExpectedContents = allInputs
			.GroupBy(x => new
			{
				ArticleId = x.ProductId,
				x.BuyPrice,
				x.CurrencyId
			})
			.Select(g => new
			{
				g.Key.ArticleId,
				g.Key.BuyPrice,
				g.Key.CurrencyId,
				Count = g.Sum(x => x.Count)
			})
			.ToList();

		foreach (var exp in groupedExpectedContents)
		{
			var actual = dbStorageContents.FirstOrDefault(x =>
				x.ProductId == exp.ArticleId && x.BuyPrice == exp.BuyPrice && x.CurrencyId == exp.CurrencyId);

			Assert.NotNull(actual);
			Assert.Equal(exp.Count, actual.Count);
			Assert.Equal(storage.Code, actual.StorageCode);
		}
	}

	private List<NewStorageContentDto> GetNewStorageContents(int count)
	{
		var products = GetContext<ProductTestContext>().Products;
		var currency = GetContext<CurrencyTestContext>().Currencies[0];
		var storage = GetContext<StorageTestContext>().Storages.First();

		return new StorageContentBuilder(Faker)
			.WithProducts(products)
			.WithCurrencyId(currency.Id)
			.WithPurchaseDate(DateTime.UtcNow)
			.WithStorageCode(storage.Code)
			.BuildMany(count)
			.Select(x => new NewStorageContentDto
			{
				BuyPrice = x.BuyPrice,
				Count = x.Count,
				CurrencyId = x.CurrencyId,
				ProductId = x.ProductId,
				PurchaseDate = x.PurchaseDatetime
			})
			.ToList();
	}
}
