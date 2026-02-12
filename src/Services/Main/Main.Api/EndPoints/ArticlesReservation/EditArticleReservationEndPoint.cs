using System.Security.Claims;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.ArticleReservations.EditArticleReservation;
using Main.Abstractions.Dtos.Amw.ArticleReservations;
using MediatR;

namespace Main.Api.EndPoints.ArticlesReservation;

public record EditArticleReservationRequest(EditArticleReservationDto NewValue);

public class EditArticleReservationEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/articles/reservations/{reservationId}", async
            (ISender sender, int reservationId, EditArticleReservationRequest request, IUserContext user,
                CancellationToken token) =>
            {
                var command = new EditArticleReservationCommand(reservationId, request.NewValue, user.UserId);
                await sender.Send(command, token);
                return Results.NoContent();
            }).WithTags("ArticleReservations")
            .WithDisplayName("Редактирование резервации")
            .WithDescription("Редактирование резервации")
            .RequireAnyPermission(PermissionCodes.ARTICLE_RESERVATIONS_EDIT);
    }
}