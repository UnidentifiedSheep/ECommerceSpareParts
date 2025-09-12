using Application.Handlers.ArticleReservations.DeleteArticleReservation;
using Carter;
using MediatR;

namespace MonoliteUnicorn.EndPoints.ArticlesReservation;

public class DeleteArticleReservationEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/articles/reservation/{reservationId}",
                async (ISender sender, int reservationId, CancellationToken cancellationToken) =>
                {
                    var command = new DeleteArticleReservationCommand(reservationId);
                    await sender.Send(command, cancellationToken);
                    return Results.NoContent();
                }).WithTags("ArticleReservations")
            .RequireAuthorization("AMW")
            .WithDisplayName("Удалить резервацию")
            .WithDescription("Удалить резервацию");
    }
}