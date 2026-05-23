using Main.Abstractions.Constants;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Entities.Event;
using Main.Entities.Exceptions;
using Main.Entities.Storage;
using Main.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.TestContainers.Combined;
using Tests.DataBuilders.Storage;
using Tests.TestContexts;
using Tests.TestContexts.Currency;
using Tests.TestContexts.Storage;
using ValidationException = FluentValidation.ValidationException;

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
        var command = new AddContentCommand([], storage.Name, StorageMovementType.StorageContentAddition);
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
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
        storageContent[^1] = storageContent[^1] with { BuyPrice = price };
        var command = new AddContentCommand(storageContent, storage.Name, StorageMovementType.StorageContentAddition);
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddContentToStorage_WithInvalidItemCount_ThrowsStorageContentCountCantBeNegativeException(
        int count)
    {
        var storage = GetContext<StorageTestContext>().Storages.First();
        var storageContent = GetNewStorageContents(3);
        storageContent[^1] = storageContent[^1] with { Count = count };
        var command = new AddContentCommand(storageContent, storage.Name,
            StorageMovementType.StorageContentAddition);
        await Assert.ThrowsAsync<ValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task AddContentToStorage_WithInvalidCurrencyId_ThrowsCurrencyNotFoundException()
    {
        var storage = GetContext<StorageTestContext>().Storages.First();
        var storageContent = GetNewStorageContents(3);
        storageContent[^1] = storageContent[^1] with { CurrencyId = int.MaxValue };
        var command = new AddContentCommand(storageContent, storage.Name,
            StorageMovementType.StorageContentAddition);
        await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));
    }

    [Fact]
    public async Task AddContentToStorage_WithInvalidStorageName_ThrowsStorageNotFoundException()
    {
        var storageContent = GetNewStorageContents(3);
        var command = new AddContentCommand(storageContent, Faker.Lorem.Letter(200),
            StorageMovementType.StorageContentAddition);
        var exception = await Assert.ThrowsAsync<DbValidationException>(async () => await Mediator.Send(command));
        Assert.Equal(ApplicationErrors.StoragesNotFound, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task AddContentToStorage_WithInvalidArticleId_ThrowsArticleNotFoundException()
    {
        var storage = GetContext<StorageTestContext>().Storages.First();
        var storageContent = GetNewStorageContents(3);
        storageContent[^1] = storageContent[^1] with { ProductId = int.MaxValue };
        var command = new AddContentCommand(storageContent, storage.Name,
            StorageMovementType.StorageContentAddition);
        await Assert.ThrowsAsync<ProductNotFoundException>(async () => await Mediator.Send(command));
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

        await AssertCorrectResult(storage, res1, res2);
    }

    private async Task<(List<NewStorageContentDto> Inputs, Dictionary<int, int> TotalPerProduct)> RunSingleAddAsync(
        Storage storage,
        IMediator mediator)
    {
        var storageContent = GetNewStorageContents(3);

        var command =
            new AddContentCommand(storageContent, storage.Name, StorageMovementType.StorageContentAddition);
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

        var dbArticles = await Context.Products
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id);

        var dbStorageContents = await Context.StorageContents.AsNoTracking()
            .Where(x => x.StorageName == storage.Name)
            .ToListAsync();

        var events = await Context.Events.OfType<StorageMovementEvent>().ToListAsync();

        foreach (var (productId, expectedTotal) in expectedTotals)
            Assert.Equal(expectedTotal, dbArticles[productId].Stock.Value);

        Assert.Equal(allInputs.Count, dbStorageContents.Count);

        var groupedExpectedContents = allInputs
            .GroupBy(x => new { ArticleId = x.ProductId, x.BuyPrice, x.CurrencyId })
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
                x.ProductId == exp.ArticleId &&
                x.BuyPrice == exp.BuyPrice &&
                x.CurrencyId == exp.CurrencyId);

            Assert.NotNull(actual);
            Assert.Equal(exp.Count, actual.Count);
            Assert.Equal(storage.Name, actual.StorageName);
        }

        Assert.Equal(dbStorageContents.Count, events.Count);
        foreach (var sc in dbStorageContents)
        {
            var match = events.FirstOrDefault(m =>
                m.Data.ProductId == sc.ProductId &&
                m.Data.Count == sc.Count &&
                m.Data.BuyPrice == sc.BuyPrice &&
                m.Data.CurrencyId == sc.CurrencyId &&
                m.Data.StorageName == sc.StorageName &&
                m.Data.MovementType == StorageMovementType.StorageContentAddition);

            Assert.NotNull(match);
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
            .WithStorageName(storage.Name)
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