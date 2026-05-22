using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductReservations.CreateProductReservation;
using Main.Application.Handlers.ProductReservations.DeleteProductReservation;
using Main.Application.Handlers.ProductReservations.EditProductReservation;
using MediatR;

namespace Main.Api.EndPoints.ProductReservations;

public record CreateProductReservationQuery(List<NewProductReservationDto> Reservations);

public record EditProductReservationRequest(EditProductReservationDto NewValue);

public class ProductReservationsEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var reservations = app.MapGroup("/products/reservations")
            .WithTags("Product Reservations");

        reservations.MapPost("", async (
                ISender sender,
                CreateProductReservationQuery query,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new CreateProductReservationCommand(query.Reservations), cancellationToken);
                return Results.NoContent();
            })
            .WithDisplayName("Создать резервацию")
            .WithDescription("Создать резервацию для пользователя")
            .RequireAnyPermission(PermissionCodes.ARTICLE_RESERVATIONS_CREATE);

        reservations.MapDelete("/{reservationId}", async (
                ISender sender,
                int reservationId,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteProductReservationCommand(reservationId), cancellationToken);
                return Results.NoContent();
            })
            .WithDisplayName("Удалить резервацию")
            .WithDescription("Удалить резервацию")
            .RequireAnyPermission(PermissionCodes.ARTICLE_RESERVATIONS_DELETE);

        reservations.MapPut("/{reservationId}", async (
                ISender sender,
                int reservationId,
                EditProductReservationRequest request,
                CancellationToken token) =>
            {
                await sender.Send(new EditProductReservationCommand(reservationId, request.NewValue), token);
                return Results.NoContent();
            })
            .WithDisplayName("Редактирование резервации")
            .WithDescription("Редактирование резервации")
            .RequireAnyPermission(PermissionCodes.ARTICLE_RESERVATIONS_EDIT);
    }
}
