using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductReservations.CreateProductReservation;
using MediatR;

namespace Main.Api.EndPoints.ArticlesReservation;

public record CreateArticleReservationQuery(List<NewProductReservationDto> Reservations);

public class CreateArticleReservationEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/reservations", async (
                ISender sender,
                IUserContext user,
                CreateArticleReservationQuery query,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateProductReservationCommand(query.Reservations, user.UserId);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("ArticleReservations")
            .WithDisplayName("Создать резервацию")
            .WithDescription("Создать резервацию для пользователя")
            .RequireAnyPermission("ARTICLE.RESERVATIONS.CREATE");
    }
}