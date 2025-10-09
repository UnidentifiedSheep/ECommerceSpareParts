using System.Security.Claims;
using Carter;
using Core.Dtos.Amw.ArticleReservations;
using Main.Application.Handlers.ArticleReservations.EditArticleReservation;
using MediatR;

namespace Main.Api.EndPoints.ArticlesReservation;

public record EditArticleReservationRequest(EditArticleReservationDto NewValue);

public class EditArticleReservationEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/articles/reservations/{reservationId}", async
            (ISender sender, int reservationId, EditArticleReservationRequest request, ClaimsPrincipal claims,
                CancellationToken token) =>
            {
                if (!Guid.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                    return Results.Unauthorized();
                var command = new EditArticleReservationCommand(reservationId, request.NewValue, userId);
                await sender.Send(command, token);
                return Results.NoContent();
            }).WithTags("ArticleReservations")
            .RequireAuthorization("AMW")
            .WithDisplayName("Редактирование резервации")
            .WithDescription("Редактирование резервации");
    }
}