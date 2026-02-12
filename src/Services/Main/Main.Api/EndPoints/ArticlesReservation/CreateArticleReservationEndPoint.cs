using System.Security.Claims;
using Abstractions.Interfaces;
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
        app.MapPost("/articles/reservations", async (ISender sender, IUserContext user,
                CreateArticleReservationQuery query, CancellationToken cancellationToken) =>
            {
                var command = new CreateArticleReservationCommand(query.Reservations, user.UserId);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("ArticleReservations")
            .WithDisplayName("Создать резервацию")
            .WithDescription("Создать резервацию для пользователя")
            .RequireAnyPermission("ARTICLE.RESERVATIONS.CREATE");
    }
}