using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.ArticlesReservation.DeleteArticleReservation;

public class DeleteArticleReservationEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/articles/reservation/{reservationId}", async (ISender sender, int reservationId, CancellationToken cancellationToken) =>
        {
            var command = new DeleteArticleReservationCommand(reservationId);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).WithGroup("ArticleReservations")
        .RequireAuthorization("AMW")
        .WithDisplayName("Удалить резервацию")
        .WithDescription("Удалить резервацию");
    }
}