using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MonoliteUnicorn.Services.JWT;
using AmwArticleDto = MonoliteUnicorn.Dtos.Amw.Articles.ArticleDto;
using AnonymousArticleDto = MonoliteUnicorn.Dtos.Anonymous.Articles.ArticleDto;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleViaExecNumber;

public record GetArticlesViaExecAmwResponse(IEnumerable<AmwArticleDto> Articles);
public record GetArticlesViaExecAnonymousResponse(IEnumerable<AnonymousArticleDto> Articles);

public record GetArticlesViaExecRequest(
    [FromQuery(Name = "searchTerm")] string SearchTerm,
    [FromQuery(Name = "page")] int Page,
    [FromQuery(Name = "viewCount")] int ViewCount,
    [FromQuery(Name = "sortBy")] string? SortBy);
public class GetArticleViaExecNumberEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/search/exec/", async (ISender sender, HttpContext context, ClaimsPrincipal user, [AsParameters] GetArticlesViaExecRequest request, CancellationToken token) =>
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
                .WithDescription("Поиск артикула по точному номеру")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithSummary("Поиск артикула по точному номеру");
    }
    
    private async Task<IResult> GetAmw(ISender sender, GetArticlesViaExecRequest request, string? userId, IEnumerable<int> producerIds,
        CancellationToken token)
    {
        var query = new GetArticleViaExecNumberAmwQuery(request.SearchTerm, request.Page, request.ViewCount, request.SortBy, producerIds, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticlesViaExecAmwResponse>();
        return Results.Ok(response);
    }
    
    private async Task<IResult> GetAnonymous(ISender sender, GetArticlesViaExecRequest request, string? userId, IEnumerable<int> producerIds,
        CancellationToken token)
    {
        var query = new GetArticleViaExecNumberAnonymousQuery(request.SearchTerm, request.Page, request.ViewCount, request.SortBy, producerIds, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticlesViaExecAnonymousResponse>();
        return Results.Ok(response);
    }
}