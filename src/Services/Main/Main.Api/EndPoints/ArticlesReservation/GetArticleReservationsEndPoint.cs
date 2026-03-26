using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.ArticleReservations;
using Main.Application.Handlers.ArticleReservations.GetArticleReservations;
using Main.Enums;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.ArticlesReservation;

public record GetArticleReservationsResponse(IEnumerable<ArticleReservationDto> Reservations);

public class GetArticleReservationsRequest
{
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "page")]
    public int Page { get; init; }

    [FromQuery(Name = "limit")]
    public int Limit { get; init; }

    [FromQuery(Name = "sortBy")]
    public string? SortBy { get; init; }

    [FromQuery(Name = "userId")]
    public Guid? UserId { get; init; }

    [FromQuery(Name = "similarity")]
    public double? Similarity { get; init; }

    [FromQuery(Name = "strategy")]
    public GeneralSearchStrategy Strategy { get; init; }
}

public class GetArticleReservationsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/reservations",
                async (ISender sender, [AsParameters] GetArticleReservationsRequest request, CancellationToken token) =>
                {
                    var query = new GetArticleReservationsQuery(
                        request.SearchTerm,
                        new PaginationModel(request.Page, request.Limit),
                        request.SortBy,
                        request.Similarity,
                        request.UserId,
                        request.Strategy);
                    var result = await sender.Send(query, token);
                    var response = result.Adapt<GetArticleReservationsResponse>();
                    return Results.Ok(response);
                })
            .WithTags("ArticleReservations")
            .WithDisplayName("Создать резервацию")
            .WithDescription("Создать резервацию для пользователя")
            .RequireAnyPermission("ARTICLE.RESERVATIONS.GET.ALL");
    }
}