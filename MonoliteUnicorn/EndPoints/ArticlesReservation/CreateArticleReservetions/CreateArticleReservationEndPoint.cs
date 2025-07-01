using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.ArticleReservations;

namespace MonoliteUnicorn.EndPoints.ArticlesReservation.CreateArticleReservetions;

public record CreateArticleReservationQuery(IEnumerable<NewArticleReservationDto> Reservations); 

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
        }).WithGroup("ArticleReservations")
        .RequireAuthorization("AMW")
        .WithDisplayName("Создать резервацию")
        .WithDescription("Создать резервацию для пользователя");
    }
}