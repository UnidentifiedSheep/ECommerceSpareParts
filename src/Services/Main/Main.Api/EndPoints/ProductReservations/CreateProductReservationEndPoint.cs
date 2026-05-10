using Api.Common.Extensions;
using Carter;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductReservations.CreateProductReservation;
using MediatR;

namespace Main.Api.EndPoints.ProductReservations;

public record CreateProductReservationQuery(List<NewProductReservationDto> Reservations);

public class CreateProductReservationEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/products/reservations", async (
                ISender sender,
                CreateProductReservationQuery query,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateProductReservationCommand(query.Reservations);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("ArticleReservations")
            .WithDisplayName("Создать резервацию")
            .WithDescription("Создать резервацию для пользователя")
            .RequireAnyPermission("ARTICLE.RESERVATIONS.CREATE");
    }
}