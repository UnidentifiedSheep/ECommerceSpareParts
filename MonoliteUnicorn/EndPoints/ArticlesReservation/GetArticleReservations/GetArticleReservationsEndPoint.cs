using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MonoliteUnicorn.Dtos.Amw.ArticleReservations;

namespace MonoliteUnicorn.EndPoints.ArticlesReservation.GetArticleReservations;

public record GetArticleReservationsResponse(IEnumerable<ArticleReservationDto> Reservations);

public class GetArticleReservationsRequest
{
    [FromQuery(Name = "searchTerm")] public string? SearchTerm { get; set; }
    [FromQuery(Name = "page")] public int Page { get; set; }
    [FromQuery(Name = "viewCount")] public int ViewCount { get; set; }
    [FromQuery(Name = "sortBy")] public string? SortBy { get; set; }
    [FromQuery(Name = "userId")] public string? UserId { get; set; }
}

public class GetArticleReservationsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/reservations",
            async (ISender sender, [AsParameters] GetArticleReservationsRequest request, CancellationToken token) =>
            {
                var query = request.Adapt<GetArticleReservationsQuery>();
                var result = await sender.Send(query, token);
                var response = result.Adapt<GetArticleReservationsResponse>();
                return Results.Ok(response);
            }).WithGroup("ArticleReservations")
            .RequireAuthorization("AMW")
            .WithDisplayName("Создать резервацию")
            .WithDescription("Создать резервацию для пользователя");
    }
}