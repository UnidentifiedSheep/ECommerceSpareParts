using Main.Abstractions.Consts;
using Main.Application.Handlers.ArticleReservations.CreateArticleReservation;
using Main.Abstractions.Dtos.Amw.ArticleReservations;
using Main.Entities;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;
using ValidationException = FluentValidation.ValidationException;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.ArticleReservations;

[Collection("Combined collection")]
public class CreateArticleReservationTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    private User _whoCreated = null!;
    private User _user = null!;
    private Article _article = null!;
    private Currency _currency = null!;

    public CreateArticleReservationTests(CombinedContainerFixture fixture)
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
        _whoCreated = await _context.Users.FirstAsync(x => x.Id != _user.Id);
        _currency = await _context.Currencies.FirstAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task CreateReservations_Success_CreatesAll()
    {
        var dto = new List<NewArticleReservationDto>
        {
            new()
            {
                ArticleId = _article.Id,
                UserId = _user.Id,
                InitialCount = 5,
                CurrentCount = 3,
                Comment = "test",
                GivenPrice = 10.25m,
                GivenCurrencyId = _currency.Id
            },
            new()
            {
                ArticleId = _article.Id,
                UserId = _user.Id,
                InitialCount = 2,
                CurrentCount = 2
            }
        };

        var cmd = new CreateArticleReservationCommand(dto, _whoCreated.Id);
        await _mediator.Send(cmd);

        var reservations = await _context.StorageContentReservations.ToListAsync();
        Assert.Equal(2, reservations.Count);
        Assert.All(reservations, r => Assert.Equal(_whoCreated.Id, r.WhoCreated));
        Assert.Contains(reservations, x => x.GivenCurrencyId == _currency.Id && x.GivenPrice == 10.25m);
    }

    [Fact]
    public async Task CreateReservations_WithMoreThan100_ThrowsValidationException()
    {
        var list = Enumerable.Range(0, 101).Select(_ => new NewArticleReservationDto
        {
            ArticleId = _article.Id,
            UserId = _user.Id,
            InitialCount = 1,
            CurrentCount = 1
        }).ToList();

        var cmd = new CreateArticleReservationCommand(list, _whoCreated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateReservations_WithNonPositiveInitialCount_ThrowsValidationException(int count)
    {
        var dto = new List<NewArticleReservationDto>
        {
            new()
            {
                ArticleId = _article.Id,
                UserId = _user.Id,
                InitialCount = count,
                CurrentCount = 1
            }
        };
        var cmd = new CreateArticleReservationCommand(dto, _whoCreated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateReservations_WithNonPositiveCurrentCount_ThrowsValidationException(int count)
    {
        var dto = new List<NewArticleReservationDto>
        {
            new()
            {
                ArticleId = _article.Id,
                UserId = _user.Id,
                InitialCount = 2,
                CurrentCount = count
            }
        };
        var cmd = new CreateArticleReservationCommand(dto, _whoCreated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Fact]
    public async Task CreateReservations_WithCurrentGreaterThanInitial_ThrowsValidationException()
    {
        var dto = new List<NewArticleReservationDto>
        {
            new()
            {
                ArticleId = _article.Id,
                UserId = _user.Id,
                InitialCount = 1,
                CurrentCount = 2
            }
        };
        var cmd = new CreateArticleReservationCommand(dto, _whoCreated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.001)]
    [InlineData(0.0001)]
    public async Task CreateReservations_WithInvalidGivenPrice_ThrowsValidationException(decimal price)
    {
        var dto = new List<NewArticleReservationDto>
        {
            new()
            {
                ArticleId = _article.Id,
                UserId = _user.Id,
                InitialCount = 2,
                CurrentCount = 2,
                GivenPrice = price,
                GivenCurrencyId = _currency.Id
            }
        };
        var cmd = new CreateArticleReservationCommand(dto, _whoCreated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Fact]
    public async Task CreateReservations_WithMissingCurrency_ThrowsCurrencyNotFound()
    {
        var dto = new List<NewArticleReservationDto>
        {
            new()
            {
                ArticleId = _article.Id,
                UserId = _user.Id,
                InitialCount = 2,
                CurrentCount = 2,
                GivenPrice = 1,
                GivenCurrencyId = int.MaxValue
            }
        };
        var cmd = new CreateArticleReservationCommand(dto, _whoCreated.Id);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
        Assert.Equal("Не удалось найти валюту.", exception.Errors.First().ErrorMessage);
    }

    [Fact]
    public async Task CreateReservations_WithMissingArticle_ThrowsArticleNotFound()
    {
        var dto = new List<NewArticleReservationDto>
        {
            new()
            {
                ArticleId = int.MaxValue,
                UserId = _user.Id,
                InitialCount = 2,
                CurrentCount = 2
            }
        };
        var cmd = new CreateArticleReservationCommand(dto, _whoCreated.Id);
        var exception = await Assert.ThrowsAsync<DbValidationException>(() => _mediator.Send(cmd));
        Assert.Equal(ApplicationErrors.ArticlesNotFound, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task CreateReservations_WithMissingUser_ThrowsUserNotFound()
    {
        var dto = new List<NewArticleReservationDto>
        {
            new()
            {
                ArticleId = _article.Id,
                UserId = Guid.NewGuid(),
                InitialCount = 2,
                CurrentCount = 2
            }
        };
        var cmd = new CreateArticleReservationCommand(dto, _whoCreated.Id);
        var exception = await Assert.ThrowsAsync<DbValidationException>(() => _mediator.Send(cmd));
        Assert.Equal(ApplicationErrors.UsersNotFound, exception.Failures[0].ErrorName);
    }
}
