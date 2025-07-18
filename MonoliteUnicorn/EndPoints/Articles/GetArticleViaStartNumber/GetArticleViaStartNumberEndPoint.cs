using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MonoliteUnicorn.Services.JWT;

using AmwArticleDto = MonoliteUnicorn.Dtos.Amw.Articles.ArticleDto;
using AnonymousArticleDto = MonoliteUnicorn.Dtos.Anonymous.Articles.ArticleDto;
namespace MonoliteUnicorn.EndPoints.Articles.GetArticleViaStartNumber;

public record GetArticleViaStartNumberAmwResponse(IEnumerable<AmwArticleDto> Articles);
public record GetArticleViaStartNumberAnonymousResponse(IEnumerable<AnonymousArticleDto> Articles);

public record GetArticleViaStartNumberRequest(
    [FromQuery(Name = "searchTerm")] string SearchTerm,
    [FromQuery(Name = "page")] int Page,
    [FromQuery(Name = "viewCount")] int ViewCount,
    [FromQuery(Name = "sortBy")] string? SortBy);
public class GetArticleViaStartNumberEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/search/start", async (ISender sender, HttpContext context, ClaimsPrincipal user, 
                [AsParameters] GetArticleViaStartNumberRequest request, CancellationToken token) =>
            {
                var userId = user.GetUserId();
                var roles = user.GetUserRoles();
                var producerIds = context.Request.Query["producerId"]
                    .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value)
                    .ToList();
                if (roles.IsAnyMatchInvariant("admin", "moderator", "worker"))
                    return await GetAmw(sender, request, userId, producerIds, token);
                return await GetAnonymous(sender, request, userId, producerIds, token);
            }).WithGroup("Articles")
                .WithDescription("Поиск артикула с начала номера")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithSummary("Поиск артикула с начала номера");
    }
    
    private async Task<IResult> GetAmw(ISender sender, GetArticleViaStartNumberRequest request, string? userId, IEnumerable<int> producerIds,
        CancellationToken token)
    {
        var query = new GetArticleViaStartNumberAmwQuery(request.SearchTerm, request.ViewCount, request.Page, request.SortBy, producerIds, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleViaStartNumberAmwResponse>();
        return Results.Ok(response);
    }
    
    private async Task<IResult> GetAnonymous(ISender sender, GetArticleViaStartNumberRequest request, string? userId, IEnumerable<int> producerIds,
        CancellationToken token)
    {
        var query = new GetArticleViaStartNumberAnonymousQuery(request.SearchTerm, request.ViewCount, request.Page, request.SortBy, producerIds, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleViaStartNumberAnonymousResponse>();
        return Results.Ok(response);
    }
}