using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MonoliteUnicorn.Services.JWT;

using AmwArticleDto = MonoliteUnicorn.Dtos.Amw.Articles.ArticleDto;
using AnonymousArticleDto = MonoliteUnicorn.Dtos.Anonymous.Articles.ArticleDto;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleByNumberOrName;

public record GetArticleByNumberOrNameAmwResponse(IEnumerable<AmwArticleDto> Articles);
public record GetArticleByNumberOrNameAnonymousResponse(IEnumerable<AnonymousArticleDto> Articles);

public record GetArticlesByNumberOrNameRequest(
    [FromQuery(Name = "searchTerm")] string SearchTerm,
    [FromQuery(Name = "page")] int Page,
    [FromQuery(Name = "viewCount")] int ViewCount,
    [FromQuery(Name = "sortBy")] string? SortBy);

public class GetArticleByNumberOrNameEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/search/full", 
            async (ISender sender, [AsParameters] GetArticlesByNumberOrNameRequest request, ClaimsPrincipal user, 
                HttpContext context, CancellationToken token) =>
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
                .WithDescription("Поиск артикулов по всем параметрам, Название, номер итд.")
                .WithDisplayName("Поиск по всем критериям");
    }

    private async Task<IResult> GetAmw(ISender sender, GetArticlesByNumberOrNameRequest request, string? userId, IEnumerable<int> producerIds,
        CancellationToken token)
    {
        var query = new GetArticleByNumberOrNameAmwQuery(request.SearchTerm, request.Page, request.ViewCount, request.SortBy, producerIds, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleByNumberOrNameAmwResponse>();
        return Results.Ok(response);
    }
    
    private async Task<IResult> GetAnonymous(ISender sender, GetArticlesByNumberOrNameRequest request, string? userId, IEnumerable<int> producerIds,
        CancellationToken token)
    {
        var query = new GetArticleByNumberOrNameAnonymousQuery(request.SearchTerm, request.Page, request.ViewCount, request.SortBy, producerIds, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleByNumberOrNameAnonymousResponse>();
        return Results.Ok(response);
    }
}