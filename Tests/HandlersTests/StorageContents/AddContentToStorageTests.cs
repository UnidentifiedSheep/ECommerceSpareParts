using Application.Configs;
using Application.Handlers.StorageContents.AddContent;
using Core.Dtos.Amw.Storage;
using Core.Entities;
using Core.Enums;
using Exceptions.Exceptions.Articles;
using Exceptions.Exceptions.Currencies;
using Exceptions.Exceptions.Storages;
using Exceptions.Exceptions.Users;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.StorageContents;

[Collection("Combined collection")]
public class AddContentToStorageTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;
    private List<Article> _articles = null!;

    private Currency _currency = null!;
    private Storage _storage = null!;
    private AspNetUser _user = null!;

    public AddContentToStorageTests(CombinedContainerFixture fixture)
    {
        MapsterConfig.Configure();
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _serviceProvider = sp;
        _mediator = sp.GetRequiredService<IMediator>();
        _context = sp.GetRequiredService<DContext>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockProducersAndArticles();
        await _mediator.AddMockStorage();
        await _mediator.AddMockUser();
        await _context.AddMockCurrencies();
        _currency = await _context.Currencies.FirstAsync();
        _storage = await _context.Storages.FirstAsync();
        _articles = await _context.Articles.ToListAsync();
        _user = await _context.AspNetUsers.FirstAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task AddContentToStorage_WithEmptyContentList_ThrowsValidationException()
    {
        var command = new AddContentCommand([], _storage.Name, _user.Id, StorageMovementType.StorageContentAddition);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0.0001)]
    [InlineData(0.001)]
    [InlineData(0)]
    public async Task AddContentToStorage_WithInvalidPrice_ThrowsStorageContentPriceCannotBeNegativeException(
        decimal price)
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        storageContent.Last().BuyPrice = price;
        var command = new AddContentCommand(storageContent, _storage.Name, _user.Id,
            StorageMovementType.StorageContentAddition);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task AddContentToStorage_WithInvalidItemCount_ThrowsStorageContentCountCantBeNegativeException(
        int count)
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        storageContent.Last().Count = count;
        var command = new AddContentCommand(storageContent, _storage.Name, _user.Id,
            StorageMovementType.StorageContentAddition);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task AddContentToStorage_WithInvalidCurrencyId_ThrowsCurrencyNotFoundException()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        storageContent.Last().CurrencyId = int.MaxValue;
        var command = new AddContentCommand(storageContent, _storage.Name, _user.Id,
            StorageMovementType.StorageContentAddition);
        await Assert.ThrowsAsync<CurrencyNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task AddContentToStorage_WithInvalidStorageName_ThrowsStorageNotFoundException()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        var command = new AddContentCommand(storageContent, Global.Faker.Lorem.Letter(200), _user.Id,
            StorageMovementType.StorageContentAddition);
        await Assert.ThrowsAsync<StorageNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task AddContentToStorage_WithInvalidUserId_ThrowsUserNotFoundException()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        var command = new AddContentCommand(storageContent, _storage.Name, Global.Faker.Lorem.Letter(10),
            StorageMovementType.StorageContentAddition);
        await Assert.ThrowsAsync<UserNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task AddContentToStorage_WithInvalidArticleId_ThrowsArticleNotFoundException()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storageContent = MockData.MockData
            .CreateNewStorageContentDto(articleIds, [_currency.Id], 10)
            .ToList();
        storageContent.Last().ArticleId = int.MaxValue;
        var command = new AddContentCommand(storageContent, _storage.Name, _user.Id,
            StorageMovementType.StorageContentAddition);
        await Assert.ThrowsAsync<ArticleNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task AddContentToStorage_Normal_Succeeds()
    {
        var result = await RunSingleAddAsync(_mediator);
        AssertCorrectResult(result);
    }

    [Fact]
    public async Task AddContentToStorage_ParallelExecution_Succeeds()
    {
        using var sc = _serviceProvider.CreateScope();
        var mediator = sc.ServiceProvider.GetRequiredService<IMediator>();
        var task1 = RunSingleAddAsync(_mediator);
        var task2 = RunSingleAddAsync(mediator);

        var results = await Task.WhenAll(task1, task2);

        var res1 = results[0];
        var res2 = results[1];

        AssertCorrectResult(res1, res2);
    }

    private async Task<(List<NewStorageContentDto> Inputs, Dictionary<int, int> TotalPerArticle)> RunSingleAddAsync(
        IMediator mediator)
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var dtoList = MockData.MockData.CreateNewStorageContentDto(articleIds, [_currency.Id], 40)
            .ToList();

        var command =
            new AddContentCommand(dtoList, _storage.Name, _user.Id, StorageMovementType.StorageContentAddition);
        await mediator.Send(command);

        var totalPerArticle = dtoList
            .GroupBy(x => x.ArticleId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Count));

        return (dtoList, totalPerArticle);
    }

    private void AssertCorrectResult(
        params (List<NewStorageContentDto> Inputs, Dictionary<int, int> TotalPerArticle)[] results)
    {
        var allInputs = results.SelectMany(r => r.Inputs).ToList();

        var expectedTotals = new Dictionary<int, int>();
        foreach (var (_, totals) in results)
        foreach (var kv in totals)
            if (!expectedTotals.TryAdd(kv.Key, kv.Value))
                expectedTotals[kv.Key] += kv.Value;

        var dbArticles = _context.Articles.AsNoTracking()
            .ToDictionaryAsync(x => x.Id).Result; // можно вызвать асинхронно, но для примера – синхронно

        var dbStorageContents = _context.StorageContents.AsNoTracking()
            .Where(x => x.StorageName == _storage.Name)
            .ToListAsync().Result;

        var dbMovements = _context.StorageMovements.AsNoTracking()
            .ToListAsync().Result;

        // Проверка Article.TotalCount
        foreach (var (articleId, expectedTotal) in expectedTotals)
            Assert.Equal(expectedTotal, dbArticles[articleId].TotalCount);

        // Проверка StorageContent
        Assert.Equal(allInputs.Count, dbStorageContents.Count);

        var groupedExpectedContents = allInputs
            .GroupBy(x => new { x.ArticleId, x.BuyPrice, x.CurrencyId })
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
                x.ArticleId == exp.ArticleId &&
                x.BuyPrice == exp.BuyPrice &&
                x.CurrencyId == exp.CurrencyId);

            Assert.NotNull(actual);
            Assert.Equal(exp.Count, actual.Count);
            Assert.Equal(_storage.Name, actual.StorageName);
        }

        // Проверка StorageMovements
        Assert.Equal(dbStorageContents.Count, dbMovements.Count);
        foreach (var sc in dbStorageContents)
        {
            var match = dbMovements.FirstOrDefault(m =>
                m.ArticleId == sc.ArticleId &&
                m.Count == sc.Count &&
                m.Price == sc.BuyPrice &&
                m.CurrencyId == sc.CurrencyId &&
                m.StorageName == sc.StorageName &&
                m.ActionType == nameof(StorageMovementType.StorageContentAddition) &&
                m.WhoMoved == _user.Id);

            Assert.NotNull(match);
        }
    }
}