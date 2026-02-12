using Main.Abstractions.Constants;
using Main.Application.Handlers.ArticleReservations.CreateArticleReservation;
using Main.Application.Handlers.ArticleReservations.SubtractCountFromReservations;
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
public class SubtractCountFromReservationsTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    private User _user = null!;
    private User _whoUpdated = null!;
    private Article _article = null!;

    public SubtractCountFromReservationsTests(CombinedContainerFixture fixture)
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
        _user = await _context.Users.FirstAsync();
        _whoUpdated = await _context.Users.FirstAsync(x => x.Id != _user.Id);

        // create two reservations for same article
        var create = new CreateArticleReservationCommand([
            new NewArticleReservationDto
            {
                ArticleId = _article.Id,
                UserId = _user.Id,
                InitialCount = 5,
                CurrentCount = 3
            },
            new NewArticleReservationDto
            {
                ArticleId = _article.Id,
                UserId = _user.Id,
                InitialCount = 4,
                CurrentCount = 4
            }
        ], _whoUpdated.Id);
        await _mediator.Send(create);
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task Subtract_EmptyInput_ReturnsEmpty()
    {
        var cmd = new SubtractCountFromReservationsCommand(_user.Id, _whoUpdated.Id, new Dictionary<int, int>());
        var result = await _mediator.Send(cmd);
        Assert.Empty(result.NotFoundReservations);
    }

    [Fact]
    public async Task Subtract_PartialAcrossMultipleReservations_UpdatesCorrectly()
    {
        var cmd = new SubtractCountFromReservationsCommand(_user.Id, _whoUpdated.Id, new Dictionary<int, int>
        {
            { _article.Id, 5 }
        });
        var result = await _mediator.Send(cmd);
        Assert.Empty(result.NotFoundReservations);

        var reservations = await _context.StorageContentReservations.Where(x => x.UserId == _user.Id && x.ArticleId == _article.Id).OrderBy(x => x.Id).ToListAsync();
        // First reservation had 3 -> should become 0 and IsDone true
        Assert.Equal(0, reservations[0].CurrentCount);
        Assert.True(reservations[0].IsDone);
        // Second had 4 -> subtract remaining 2, should become 2
        Assert.Equal(2, reservations[1].CurrentCount);
        Assert.Equal(_whoUpdated.Id, reservations[0].WhoUpdated);
        Assert.Equal(_whoUpdated.Id, reservations[1].WhoUpdated);
        Assert.All(reservations, r => Assert.NotNull(r.UpdatedAt));
    }

    [Fact]
    public async Task Subtract_MoreThanExists_ReturnsNotFoundRemainder()
    {
        var cmd = new SubtractCountFromReservationsCommand(_user.Id, _whoUpdated.Id, new Dictionary<int, int>
        {
            { _article.Id, 20 }
        });
        var result = await _mediator.Send(cmd);
        Assert.True(result.NotFoundReservations.TryGetValue(_article.Id, out var remainder) && remainder > 0);

        var totalLeft = await _context.StorageContentReservations.Where(x => x.UserId == _user.Id && x.ArticleId == _article.Id).SumAsync(x => x.CurrentCount);
        Assert.Equal(0, totalLeft);
    }

    [Fact]
    public async Task Subtract_ZeroOrNegativeCounts_IgnoredAndNotReturned()
    {
        var cmd = new SubtractCountFromReservationsCommand(_user.Id, _whoUpdated.Id, new Dictionary<int, int>
        {
            { _article.Id, 0 },
            { int.MaxValue, -5 }
        });
        var result = await _mediator.Send(cmd);
        Assert.Empty(result.NotFoundReservations);
    }

    [Fact]
    public async Task Subtract_UserNotFound_Throws()
    {
        var cmd = new SubtractCountFromReservationsCommand(Guid.NewGuid(), _whoUpdated.Id, new Dictionary<int, int>
        {
            { _article.Id, 1 }
        });
        var exception = await Assert.ThrowsAsync<DbValidationException>(() => _mediator.Send(cmd));
        Assert.Equal(ApplicationErrors.UsersNotFound, exception.Failures[0].ErrorName);
    }
}
