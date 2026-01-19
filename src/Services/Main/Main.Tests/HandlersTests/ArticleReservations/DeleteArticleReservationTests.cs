using Exceptions.Exceptions.ArticleReservations;
using Main.Application.Handlers.ArticleReservations.CreateArticleReservation;
using Main.Application.Handlers.ArticleReservations.DeleteArticleReservation;
using Main.Abstractions.Dtos.Amw.ArticleReservations;
using Main.Entities;
using Main.Persistence.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.MockData;
using Tests.testContainers.Combined;

namespace Tests.HandlersTests.ArticleReservations;

[Collection("Combined collection")]
public class DeleteArticleReservationTests : IAsyncLifetime
{
    private readonly DContext _context;
    private readonly IMediator _mediator;

    private User _user = null!;
    private User _whoCreated = null!;
    private Article _article = null!;

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

        _article = await _context.Articles.FirstAsync();
        _user = await _context.Users.FirstAsync();
        _whoCreated = await _context.Users.FirstAsync(x => x.Id != _user.Id);

        var create = new CreateArticleReservationCommand([
            new NewArticleReservationDto
            {
                ArticleId = _article.Id,
                UserId = _user.Id,
                InitialCount = 2,
                CurrentCount = 2
            }
        ], _whoCreated.Id);
        await _mediator.Send(create);
    }

    public async Task DisposeAsync()
    {
        await _context.ClearDatabaseFull();
    }

    [Fact]
    public async Task DeleteReservation_Success_RemovesEntity()
    {
        var reservation = await _context.StorageContentReservations.FirstAsync();
        var cmd = new DeleteArticleReservationCommand(reservation.Id);
        await _mediator.Send(cmd);

        var exists = await _context.StorageContentReservations.AnyAsync(x => x.Id == reservation.Id);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteReservation_NotFound_Throws()
    {
        var cmd = new DeleteArticleReservationCommand(int.MaxValue);
        await Assert.ThrowsAsync<ReservationNotFoundException>(() => _mediator.Send(cmd));
    }
}
