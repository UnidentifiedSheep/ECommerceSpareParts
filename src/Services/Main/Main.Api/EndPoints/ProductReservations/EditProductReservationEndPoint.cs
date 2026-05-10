using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductReservations.EditProductReservation;
using MediatR;

namespace Main.Api.EndPoints.ProductReservations;

public record EditProductReservationRequest(EditProductReservationDto NewValue);

public class EditProductReservationEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/products/reservations/{reservationId}", async (
                ISender sender,
                int reservationId,
                EditProductReservationRequest request,
                CancellationToken token) =>
            {
                var command = new EditProductReservationCommand(reservationId, request.NewValue);
                await sender.Send(command, token);
                return Results.NoContent();
            }).WithTags("ArticleReservations")
            .WithDisplayName("Редактирование резервации")
            .WithDescription("Редактирование резервации")
            .RequireAnyPermission(PermissionCodes.ARTICLE_RESERVATIONS_EDIT);
    }
}