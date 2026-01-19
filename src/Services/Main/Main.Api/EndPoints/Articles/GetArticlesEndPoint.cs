using System.Security.Claims;
using Carter;
using Core.Models;
using Main.Application.Handlers.Articles.GetArticles;
using Main.Enums;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Security.Extensions;
using AmwArticleDto = Main.Abstractions.Dtos.Amw.Articles.ArticleDto;
using AnonymousArticleDto = Main.Abstractions.Dtos.Anonymous.Articles.ArticleDto;

namespace Main.Api.EndPoints.Articles;

public record GetArticleAmwResponse(IEnumerable<AmwArticleDto> Articles);

public record GetArticleAnonymousResponse(IEnumerable<AnonymousArticleDto> Articles);

public record GetArticleRequest(
    [FromQuery(Name = "searchTerm")] string SearchTerm,
    [FromQuery(Name = "page")] int Page,
    [FromQuery(Name = "limit")] int Limit,
    [FromQuery(Name = "sortBy")] string? SortBy,
    [FromQuery(Name = "searchStrategy")] ArticleSearchStrategy SearchStrategy);

public class GetArticlesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles", async (ISender sender, HttpContext context, ClaimsPrincipal user,
                [AsParameters] GetArticleRequest request, CancellationToken token) =>
            {
                user.GetUserId(out var userId);
                
                var producerIds = context.Request.Query["producerId"]
                    .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .ToList();
                var pagination = new PaginationModel(request.Page, request.Limit);
                if (user.HasPermissions("ARTICLES.GET.FULL"))
                    return await GetAmw(sender, request, pagination, userId, producerIds, token);
                return await GetAnonymous(sender, request, pagination, userId, producerIds, token);
            }).WithTags("Articles")
            .WithDescription("Поиск артикула с начала номера")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Поиск артикула с начала номера");
    }

    private async Task<IResult> GetAmw(ISender sender, GetArticleRequest request,
        PaginationModel pagination,
        Guid? userId, IEnumerable<int> producerIds, CancellationToken token)
    {
        var query = new GetArticlesQuery<AmwArticleDto>(request.SearchTerm, pagination, request.SortBy, producerIds,
            request.SearchStrategy, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleAmwResponse>();
        return Results.Ok(response);
    }

    private async Task<IResult> GetAnonymous(ISender sender, GetArticleRequest request,
        PaginationModel pagination,
        Guid? userId, IEnumerable<int> producerIds, CancellationToken token)
    {
        var query = new GetArticlesQuery<AnonymousArticleDto>(request.SearchTerm, pagination, request.SortBy,
            producerIds, request.SearchStrategy, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleAnonymousResponse>();
        return Results.Ok(response);
    }
}