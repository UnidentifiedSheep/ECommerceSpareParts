using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.ProductReservations.DeleteProductReservation;
using MediatR;

namespace Main.Api.EndPoints.ProductReservations;

public class DeleteProductReservationEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/products/reservations/{reservationId}",
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