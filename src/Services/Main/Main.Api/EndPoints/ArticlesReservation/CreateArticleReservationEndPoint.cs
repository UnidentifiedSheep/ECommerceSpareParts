using System.Security.Claims;
using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.ArticleReservations.CreateArticleReservation;
using Main.Abstractions.Dtos.Amw.ArticleReservations;
using MediatR;

namespace Main.Api.EndPoints.ArticlesReservation;

public record CreateArticleReservationQuery(List<NewArticleReservationDto> Reservations);

public class CreateArticleReservationEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/articles/reservations", async (ISender sender, ClaimsPrincipal claims,
                CreateArticleReservationQuery query, CancellationToken cancellationToken) =>
            {
                if (!Guid.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Results.Unauthorized();
                var command = new CreateArticleReservationCommand(query.Reservations, userId);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("ArticleReservations")
            .WithDisplayName("Создать резервацию")
            .WithDescription("Создать резервацию для пользователя")
            .RequireAnyPermission("ARTICLE.RESERVATIONS.CREATE");
    }
}