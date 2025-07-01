using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using MediatR;
using MonoliteUnicorn.Dtos.Amw.ArticleReservations;

namespace MonoliteUnicorn.EndPoints.ArticlesReservation.EditArticleReservation;

public record EditArticleReservationRequest(EditArticleReservationDto NewValue); 
    
public class EditArticleReservationEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/articles/reservations/{reservationId}", async
        (ISender sender, int reservationId, EditArticleReservationRequest request, ClaimsPrincipal claims,
            CancellationToken token) =>
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Results.Unauthorized();
            var command = new EditArticleReservationCommand(reservationId, request.NewValue, userId);
            await sender.Send(command, token);
            return Results.NoContent();
        }).WithGroup("ArticleReservations")
        .RequireAuthorization("AMW")
        .WithDisplayName("Редактирование резервации")
        .WithDescription("Редактирование резервации");
    }
}