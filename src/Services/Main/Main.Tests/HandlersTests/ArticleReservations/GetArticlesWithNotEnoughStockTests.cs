using Main.Abstractions.Consts;
using Main.Application.Handlers.ArticleReservations.CreateArticleReservation;
using Main.Application.Handlers.ArticleReservations.GetArticlesWithNotEnoughStock;
using Main.Abstractions.Dtos.Amw.ArticleReservations;
using Main.Entities;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.ArticleReservations;

[Collection("Combined collection")]
public class GetArticlesWithNotEnoughStockTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    private User _buyer = null!;
    private User _otherUser = null!;
    private Article _article = null!;
    private Currency _currency = null!;
    private string _storageName = null!;

    public GetArticlesWithNotEnoughStockTests(CombinedContainerFixture fixture)
    {
        var sp = ServiceProviderForTests.Build(fixture.PostgresConnectionString, fixture.RedisConnectionString);
        _context = sp.GetRequiredService<DContext>();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    public async Task InitializeAsync()
    {
        await _mediator.AddMockProducersAndArticles();
        await _mediator.AddMockUser();
        await _mediator.AddMockUser();
        await _mediator.AddMockStorage();
        await _context.AddMockCurrencies();

        _article = await _context.Articles.FirstAsync();
        _buyer = await _context.Users.FirstAsync();
        _otherUser = await _context.Users.FirstAsync(x => x.Id != _buyer.Id);
        _currency = await _context.Currencies.FirstAsync();
        _storageName = (await _context.Storages.FirstAsync()).Name;

        // Seed storage contents: 10 storage content rows (random counts) for the article; rely on Article.TotalCount for stock
        await _mediator.AddMockStorageContents([_article.Id], _currency.Id, _storageName, _buyer.Id, 10);

        // Create reservations: buyer reserves 1, other user reserves 5
        await _mediator.Send(new CreateArticleReservationCommand([
            new NewArticleReservationDto
            {
                ArticleId = _article.Id,
                UserId = _buyer.Id,
                InitialCount = 1,
                CurrentCount = 1
            },
            new NewArticleReservationDto
            {
                ArticleId = _article.Id,
                UserId = _otherUser.Id,
                InitialCount = 5,
                CurrentCount = 5
            }
        ], _buyer.Id));
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task NotEnoughByStock_WhenNeededExceedsStorage()
    {
        var articleCount = await _context.Articles.AsNoTracking().FirstAsync(x => x.Id == _article.Id);

        var query = new GetArticlesWithNotEnoughStockQuery(_buyer.Id, _storageName, false, new Dictionary<int, int>
        {
            { _article.Id, articleCount.TotalCount + 15 } // deficit is 15
        });
        var result = await _mediator.Send(query);
        var found = result.NotEnoughByStock.TryGetValue(_article.Id, out var deficit);
        Assert.True(found && deficit == 15);
        Assert.Empty(result.NotEnoughByReservation);
    }

    [Fact]
    public async Task NotEnoughByReservation_WhenOthersReservationsMakeItInsufficient()
    {
        var article = await _context.Articles.AsNoTracking().FirstAsync(x => x.Id == _article.Id);
        // Needed 8, stock 10 -> stockDiff = 2; others 5; user 1 -> reservationsDiff = 2 - 5 + 1 = -2
        var query = new GetArticlesWithNotEnoughStockQuery(_buyer.Id, _storageName, false, new Dictionary<int, int>
        {
            { _article.Id, article.TotalCount - 2 }
        });
        var result = await _mediator.Send(query);
        Assert.True(result.NotEnoughByReservation.TryGetValue(_article.Id, out var deficit) && deficit == 2);
        Assert.Empty(result.NotEnoughByStock);
    }

    [Fact]
    public async Task ValidationErrors_ForMissingData()
    {
        var badStorage = new GetArticlesWithNotEnoughStockQuery(_buyer.Id, "__no_storage__", false, new Dictionary<int, int> { { _article.Id, 1 } });
        var exception = await Assert.ThrowsAsync<DbValidationException>(() => _mediator.Send(badStorage));
        Assert.Equal(ApplicationErrors.StoragesNotFound, exception.Failures[0].ErrorName);
        
        var badUser = new GetArticlesWithNotEnoughStockQuery(Guid.NewGuid(), _storageName, false, new Dictionary<int, int> { { _article.Id, 1 } });
        exception = await Assert.ThrowsAsync<DbValidationException>(() => _mediator.Send(badUser));
        Assert.Equal(ApplicationErrors.UsersNotFound, exception.Failures[0].ErrorName);
        
        var badArticle = new GetArticlesWithNotEnoughStockQuery(_buyer.Id, _storageName, false, new Dictionary<int, int> { { int.MaxValue, 1 } });
        exception = await Assert.ThrowsAsync<DbValidationException>(() => _mediator.Send(badArticle));
        Assert.Equal(ApplicationErrors.ArticlesNotFound, exception.Failures[0].ErrorName);
    }
}
