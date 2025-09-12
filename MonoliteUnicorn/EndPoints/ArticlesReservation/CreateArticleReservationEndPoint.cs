using System.Security.Claims;
using Application.Handlers.ArticleReservations.CreateArticleReservation;
using Carter;
using Core.Dtos.Amw.ArticleReservations;
using MediatR;

namespace MonoliteUnicorn.EndPoints.ArticlesReservation;

public record CreateArticleReservationQuery(List<NewArticleReservationDto> Reservations); 

public class CreateArticleReservationEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/reservations", async (ISender sender, ClaimsPrincipal claims, CreateArticleReservationQuery query, CancellationToken cancellationToken) =>
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Results.Unauthorized();
            var command = new CreateArticleReservationCommand(query.Reservations, userId);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).WithTags("ArticleReservations")
        .RequireAuthorization("AMW")
        .WithDisplayName("Создать резервацию")
        .WithDescription("Создать резервацию для пользователя");
    }
}