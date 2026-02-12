using Exceptions.Exceptions.Articles;
using Main.Abstractions.Constants;
using Main.Application.Handlers.StorageContents.RestoreContent;
using Main.Abstractions.Dtos.Amw.Sales;
using Main.Entities;
using Main.Enums;
using Main.Abstractions.Models;
using Main.Persistence.Context;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using ValidationException = FluentValidation.ValidationException;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.StorageContents;

[Collection("Combined collection")]
public class RestoreContentToStorageTests : IAsyncLifetime
{
    private const int ContentGenerationCount = 10;
    private const int InvalidStorageNameLength = 100;
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private List<Article> _articles = null!;

    private List<Currency> _currency = null!;
    private Dictionary<int, StorageContent> _storageContent = null!;
    private List<Storage> _storages = null!;
    private User _user = null!;

    public RestoreContentToStorageTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockUser();
        await _mediator.AddMockProducersAndArticles();
        await _mediator.AddMockStorage();
        await _context.AddMockCurrencies();

        _user = await _context.Users.FirstAsync();
        _articles = await _context.Articles.ToListAsync();
        _storages = await _context.Storages.ToListAsync();
        _currency = await _context.Currencies.ToListAsync();
        var articleIds = _articles.Select(a => a.Id);
        var storage = await _context.Storages.FirstAsync();
        var currency = await _context.Currencies.FirstAsync();

        await _mediator.AddMockStorageContents(articleIds, currency.Id, storage.Name, _user.Id, 10);
        _storageContent = await _context.StorageContents.ToDictionaryAsync(x => x.Id);
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    private List<RestoreContentItem> GenerateValidStorageContent()
    {
        var articleIds = _articles.Select(x => x.Id).ToArray();
        var storages = _storages.Select(x => x.Name).ToArray();
        var currencies = _currency.Select(x => x.Id).ToArray();

        var storageContent = MockData.MockData
            .CreateSaleContentDetails(_storageContent.Keys, storages, currencies, ContentGenerationCount)
            .Select(x => new RestoreContentItem(x.Adapt<SaleContentDetailDto>(),
                x.StorageContentId == null
                    ? Global.Faker.PickRandom(articleIds)
                    : _storageContent[x.StorageContentId.Value].ArticleId))
            .ToList();
        foreach (var (detail, _) in storageContent.Where(x => x.Detail.StorageContentId != null))
            detail.Storage = _storageContent[detail.StorageContentId!.Value].StorageName;
        return storageContent;
    }

    [Fact]
    public async Task RestoreContentToStorage_WithEmptyContentList_ThrowsArgumentException()
    {
        var command = new RestoreContentCommand([], StorageMovementType.StorageContentAddition, _user.Id);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RestoreContentToStorage_WithInvalidPrice_ThrowsStorageContentPriceCannotBeNegativeException(
        decimal invalidPrice)
    {
        var content = GenerateValidStorageContent();
        content[^1].Detail.BuyPrice = invalidPrice;

        var command = new RestoreContentCommand(content, StorageMovementType.StorageContentAddition, _user.Id);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task RestoreContentToStorage_WithInvalidCount_ThrowsStorageContentCountCantBeNegativeException(
        int count)
    {
        var content = GenerateValidStorageContent();
        content[^1].Detail.Count = count;

        var command = new RestoreContentCommand(content, StorageMovementType.StorageContentAddition, _user.Id);
        await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public async Task RestoreContentToStorage_WithInvalidCurrencyId_ThrowsCurrencyNotFoundException(int currencyId)
    {
        var content = GenerateValidStorageContent();
        content[^1].Detail.CurrencyId = currencyId;

        var command = new RestoreContentCommand(content, StorageMovementType.StorageContentAddition, _user.Id);
        var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _mediator.Send(command));
        Assert.Equal("Не удалось найти валюту.", exception.Errors.First().ErrorMessage);
    }

    [Fact]
    public async Task RestoreContentToStorage_WithInvalidStorageName_ThrowsStorageNotFoundException()
    {
        var content = GenerateValidStorageContent();
        content[^1].Detail.Storage = Global.Faker.Lorem.Letter(InvalidStorageNameLength);

        var command = new RestoreContentCommand(content, StorageMovementType.StorageContentAddition, _user.Id);
        var exception = await Assert.ThrowsAsync<DbValidationException>(async () => await _mediator.Send(command));
        Assert.Equal(ApplicationErrors.StoragesNotFound, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task RestoreContentToStorage_WithInvalidUserId_ThrowsUserNotFoundException()
    {
        var content = GenerateValidStorageContent();
        var invalidUserId = Global.Faker.Random.Guid();

        var command = new RestoreContentCommand(content, StorageMovementType.StorageContentAddition, invalidUserId);
        var exception = await Assert.ThrowsAsync<DbValidationException>(async () => await _mediator.Send(command));
        Assert.Equal(ApplicationErrors.UsersNotFound, exception.Failures[0].ErrorName);
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public async Task RestoreContentToStorage_WithInvalidArticleId_ThrowsArticleNotFoundException(int invalidArticleId)
    {
        var content = GenerateValidStorageContent();
        content[^1] = new RestoreContentItem(content[^1].Detail, invalidArticleId);

        var command = new RestoreContentCommand(content, StorageMovementType.StorageContentAddition, _user.Id);
        await Assert.ThrowsAsync<ArticleNotFoundException>(async () => await _mediator.Send(command));
    }

    [Fact]
    public async Task RestoreContentToStorage_WithValidData_Succeeds()
    {
        var content = GenerateValidStorageContent();
        var nullIdsCount = content.Count(x => x.Detail.StorageContentId == null);

        var command = new RestoreContentCommand(content, StorageMovementType.StorageContentAddition, _user.Id);
        await _mediator.Send(command);

        var storageContents = await _context.StorageContents
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id);

        var articles = await _context.Articles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id);


        Assert.Equal(_storageContent.Count + nullIdsCount, storageContents.Count);
        foreach (var (saleContent, articleId) in content)
        {
            var dbStorageContent = saleContent.StorageContentId == null
                ? storageContents.FirstOrDefault(x => x.Value.ArticleId == articleId &&
                                                      x.Value.Count == saleContent.Count &&
                                                      x.Value.StorageName == saleContent.Storage).Value
                : storageContents[saleContent.StorageContentId.Value];

            Assert.NotNull(dbStorageContent);
            Assert.Equal(articleId, dbStorageContent.ArticleId);

            if (saleContent.StorageContentId == null)
                Assert.Equal(saleContent.BuyPrice, dbStorageContent.BuyPrice);
        }

        foreach (var i in storageContents)
            articles[i.Value.ArticleId].TotalCount -= i.Value.Count;

        Assert.All(articles, x => Assert.Equal(0, x.Value.TotalCount));
    }
}