using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.ProductReservations.DeleteProductReservation;
using MediatR;

namespace Main.Api.EndPoints.ArticlesReservation;

public class DeleteArticleReservationEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/articles/reservations/{reservationId}",
                async (ISender sender, int reservationId, CancellationToken cancellationToken) =>
                {
                    var command = new DeleteProductReservationCommand(reservationId);
                    await sender.Send(command, cancellationToken);
                    return Results.NoContent();
                }).WithTags("ArticleReservations")
            .WithDisplayName("Удалить резервацию")
            .WithDescription("Удалить резервацию")
            .RequireAnyPermission("ARTICLE.RESERVATIONS.DELETE");
    }
}