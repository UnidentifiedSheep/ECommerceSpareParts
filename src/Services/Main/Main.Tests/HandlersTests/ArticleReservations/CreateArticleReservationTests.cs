using Main.Abstractions.Constants;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductReservations.CreateProductReservation;
using Main.Entities;
using Main.Entities.Currency;
using Main.Entities.Product;
using Main.Entities.User;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.MockData;
using ValidationException = FluentValidation.ValidationException;
using DbValidationException = BulkValidation.Core.Exceptions.ValidationException;

namespace Tests.HandlersTests.ArticleReservations;

[Collection("Combined collection")]
public class CreateArticleReservationTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private Product _product = null!;
    private Currency _currency = null!;
    private User _user = null!;

    private User _whoCreated = null!;

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

        _product = await _context.Products.FirstAsync();
        _user = await _context.Users.FirstAsync();
        _whoCreated = await _context.Users.FirstAsync(x => x.Id != _user.Id);
        _currency = await _context.Currencies.FirstAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabase();
    }

    [Fact]
    public async Task CreateReservations_Success_CreatesAll()
    {
        var dto = new List<NewProductReservationDto>
        {
            new()
            {
                ProductId = _product.Id,
                UserId = _user.Id,
                ReservedCount = 5,
                CurrentCount = 3,
                Comment = "test",
                ProposedPrice = 10.25m,
                GivenCurrencyId = _currency.Id
            },
            new()
            {
                ProductId = _product.Id,
                UserId = _user.Id,
                ReservedCount = 2,
                CurrentCount = 2
            }
        };

        var cmd = new CreateProductReservationCommand(dto, _whoCreated.Id);
        await _mediator.Send(cmd);

        var reservations = await _context.StorageContentReservations.ToListAsync();
        Assert.Equal(2, reservations.Count);
        Assert.All(reservations, r => Assert.Equal(_whoCreated.Id, r.WhoCreated));
        Assert.Contains(reservations, x => x.ProposedCurrencyId == _currency.Id && x.ProposedPrice == 10.25m);
    }

    [Fact]
    public async Task CreateReservations_WithMoreThan100_ThrowsValidationException()
    {
        var list = Enumerable.Range(0, 101).Select(_ => new NewProductReservationDto
        {
            ProductId = _product.Id,
            UserId = _user.Id,
            ReservedCount = 1,
            CurrentCount = 1
        }).ToList();

        var cmd = new CreateProductReservationCommand(list, _whoCreated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateReservations_WithNonPositiveInitialCount_ThrowsValidationException(int count)
    {
        var dto = new List<NewProductReservationDto>
        {
            new()
            {
                ProductId = _product.Id,
                UserId = _user.Id,
                ReservedCount = count,
                CurrentCount = 1
            }
        };
        var cmd = new CreateProductReservationCommand(dto, _whoCreated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateReservations_WithNonPositiveCurrentCount_ThrowsValidationException(int count)
    {
        var dto = new List<NewProductReservationDto>
        {
            new()
            {
                ProductId = _product.Id,
                UserId = _user.Id,
                ReservedCount = 2,
                CurrentCount = count
            }
        };
        var cmd = new CreateProductReservationCommand(dto, _whoCreated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Fact]
    public async Task CreateReservations_WithCurrentGreaterThanInitial_ThrowsValidationException()
    {
        var dto = new List<NewProductReservationDto>
        {
            new()
            {
                ProductId = _product.Id,
                UserId = _user.Id,
                ReservedCount = 1,
                CurrentCount = 2
            }
        };
        var cmd = new CreateProductReservationCommand(dto, _whoCreated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.001)]
    [InlineData(0.0001)]
    public async Task CreateReservations_WithInvalidGivenPrice_ThrowsValidationException(decimal price)
    {
        var dto = new List<NewProductReservationDto>
        {
            new()
            {
                ProductId = _product.Id,
                UserId = _user.Id,
                ReservedCount = 2,
                CurrentCount = 2,
                ProposedPrice = price,
                GivenCurrencyId = _currency.Id
            }
        };
        var cmd = new CreateProductReservationCommand(dto, _whoCreated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Fact]
    public async Task CreateReservations_WithMissingCurrency_ThrowsCurrencyNotFound()
    {
        var dto = new List<NewProductReservationDto>
        {
            new()
            {
                ProductId = _product.Id,
                UserId = _user.Id,
                ReservedCount = 2,
                CurrentCount = 2,
                ProposedPrice = 1,
                GivenCurrencyId = int.MaxValue
            }
        };
        var cmd = new CreateProductReservationCommand(dto, _whoCreated.Id);
        await Assert.ThrowsAsync<ValidationException>(() => _mediator.Send(cmd));
    }

    [Fact]
    public async Task CreateReservations_WithMissingArticle_ThrowsArticleNotFound()
    {
        var dto = new List<NewProductReservationDto>
        {
            new()
            {
                ProductId = int.MaxValue,
                UserId = _user.Id,
                ReservedCount = 2,
                CurrentCount = 2
            }
        };
        var cmd = new CreateProductReservationCommand(dto, _whoCreated.Id);
        var exception = await Assert.ThrowsAsync<DbValidationException>(() => _mediator.Send(cmd));
        Assert.Equal(ApplicationErrors.ArticlesNotFound, exception.Failures[0].ErrorName);
    }

    [Fact]
    public async Task CreateReservations_WithMissingUser_ThrowsUserNotFound()
    {
        var dto = new List<NewProductReservationDto>
        {
            new()
            {
                ProductId = _product.Id,
                UserId = Guid.NewGuid(),
                ReservedCount = 2,
                CurrentCount = 2
            }
        };
        var cmd = new CreateProductReservationCommand(dto, _whoCreated.Id);
        var exception = await Assert.ThrowsAsync<DbValidationException>(() => _mediator.Send(cmd));
        Assert.Equal(ApplicationErrors.UsersNotFound, exception.Failures[0].ErrorName);
    }
}