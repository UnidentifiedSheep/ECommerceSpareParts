using Exceptions.Exceptions.ArticleReservations;
using Main.Abstractions.Consts;
using Main.Application.Handlers.ArticleReservations.CreateArticleReservation;
using Main.Application.Handlers.ArticleReservations.EditArticleReservation;
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
public class EditArticleReservationTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    private User _user = null!;
    private User _whoUpdated = null!;
    private Article _article = null!;
    private Currency _currency = null!;

    private int _reservationId;

    public EditArticleReservationTests(CombinedContainerFixture fixture)
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
        _currency = await _context.Currencies.FirstAsync();

        var create = new CreateArticleReservationCommand([
            new NewArticleReservationDto
            {
                ArticleId = _article.Id,
                UserId = _user.Id,
                InitialCount = 4,
                CurrentCount = 3
            }
        ], _whoUpdated.Id);
        await _mediator.Send(create);
        _reservationId = (await _context.StorageContentReservations.FirstAsync()).Id;
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task EditReservation_Success_UpdatesFields()
    {
        var dto = new EditArticleReservationDto
        {
            ArticleId = _article.Id,
            InitialCount = 5,
            CurrentCount = 1,
            GivenPrice = 9.99m,
            GivenCurrencyId = _currency.Id,
            Comment = "updated"
        };

        var cmd = new EditArticleReservationCommand(_reservationId, dto, _whoUpdated.Id);
        await _mediator.Send(cmd);

        var updated = await _context.StorageContentReservations.FirstAsync(x => x.Id == _reservationId);
        Assert.Equal(5, updated.InitialCount);
        Assert.Equal(1, updated.CurrentCount);
        Assert.Equal(9.99m, updated.GivenPrice);
        Assert.Equal(_currency.Id, updated.GivenCurrencyId);
        Assert.Equal("updated", updated.Comment);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task EditReservation_InvalidInitialCount_Validation(int initial)
    {
        var dto = new EditArticleReservationDto
        {
            ArticleId = _article.Id,
            InitialCount = initial,
            CurrentCount = 1
        };
        var cmd = new EditArticleReservationCommand(_reservationId, dto, _whoUpdated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Fact]
    public async Task EditReservation_CurrentGreaterThanInitial_Validation()
    {
        var dto = new EditArticleReservationDto
        {
            ArticleId = _article.Id,
            InitialCount = 1,
            CurrentCount = 2
        };
        var cmd = new EditArticleReservationCommand(_reservationId, dto, _whoUpdated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.001)]
    [InlineData(0.0001)]
    public async Task EditReservation_InvalidPrice_Validation(decimal price)
    {
        var dto = new EditArticleReservationDto
        {
            ArticleId = _article.Id,
            InitialCount = 2,
            CurrentCount = 1,
            GivenPrice = price,
            GivenCurrencyId = _currency.Id
        };
        var cmd = new EditArticleReservationCommand(_reservationId, dto, _whoUpdated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Fact]
    public async Task EditReservation_MissingReservation_Throws()
    {
        var dto = new EditArticleReservationDto
        {
            ArticleId = _article.Id,
            InitialCount = 2,
            CurrentCount = 1
        };
        var cmd = new EditArticleReservationCommand(int.MaxValue, dto, _whoUpdated.Id);
        await Assert.ThrowsAsync<ReservationNotFoundException>(() => _mediator.Send(cmd));
    }

    [Fact]
    public async Task EditReservation_MissingArticle_Throws()
    {
        var dto = new EditArticleReservationDto
        {
            ArticleId = int.MaxValue,
            InitialCount = 2,
            CurrentCount = 1
        };
        var cmd = new EditArticleReservationCommand(_reservationId, dto, _whoUpdated.Id);
        var exception = await Assert.ThrowsAsync<DbValidationException>(() => _mediator.Send(cmd));
        Assert.Equal(ApplicationErrors.ArticlesNotFound, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task EditReservation_MissingCurrency_Throws()
    {
        var dto = new EditArticleReservationDto
        {
            ArticleId = _article.Id,
            InitialCount = 2,
            CurrentCount = 1,
            GivenPrice = 1,
            GivenCurrencyId = int.MaxValue
        };
        var cmd = new EditArticleReservationCommand(_reservationId, dto, _whoUpdated.Id);
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
        Assert.Equal("Не удалось найти валюту.", exception.Errors.First().ErrorMessage);
    }
}
