using Main.Abstractions.Exceptions.Articles;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductReservations.CreateProductReservation;
using Main.Application.Handlers.ProductReservations.DeleteProductReservation;
using Main.Entities;
using Main.Entities.Product;
using Main.Entities.User;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Test.Common.Extensions;
using Test.Common.TestContainers.Combined;
using Tests.MockData;

namespace Tests.HandlersTests.ArticleReservations;

[Collection("Combined collection")]
public class DeleteArticleReservationTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;
    private Product _product = null!;

    private User _user = null!;
    private User _whoCreated = null!;

    public DeleteArticleReservationTests(CombinedContainerFixture fixture)
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

        var create = new CreateProductReservationCommand([
            new NewProductReservationDto
            {
                ProductId = _product.Id,
                UserId = _user.Id,
                ReservedCount = 2,
                CurrentCount = 2
            }
        ], _whoCreated.Id);
        await _mediator.Send(create);
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabase();
    }

    [Fact]
    public async Task DeleteReservation_Success_RemovesEntity()
    {
        var reservation = await _context.StorageContentReservations.FirstAsync();
        var cmd = new DeleteProductReservationCommand(reservation.Id);
        await _mediator.Send(cmd);

        var exists = await _context.StorageContentReservations.AnyAsync(x => x.Id == reservation.Id);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteReservation_NotFound_Throws()
    {
        var cmd = new DeleteProductReservationCommand(int.MaxValue);
        await Assert.ThrowsAsync<ReservationNotFoundException>(() => _mediator.Send(cmd));
    }
}