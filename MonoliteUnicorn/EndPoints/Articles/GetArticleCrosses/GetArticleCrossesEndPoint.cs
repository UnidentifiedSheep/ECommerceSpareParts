using System.Security.Claims;
using Carter;
using Core.StaticFunctions;
using Mapster;
using MediatR;

using AmwArticleDto = MonoliteUnicorn.Dtos.Amw.Articles.ArticleFullDto;
using MemberArticleDto = MonoliteUnicorn.Dtos.Member.Articles.ArticleFullDto;

namespace MonoliteUnicorn.EndPoints.Articles.GetArticleCrosses;

public record GetArticleCrossesAmwResponse(IEnumerable<AmwArticleDto> Crosses, AmwArticleDto RequestedArticle);
public record GetArticleCrossesMemberResponse(IEnumerable<MemberArticleDto> Crosses, MemberArticleDto RequestedArticle);

public class GetArticleCrossesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/crosses/{articleId}", async (ISender sender, ClaimsPrincipal user, int articleId, int? currencyId, int viewCount, int page, string? sortBy, CancellationToken token) =>
        {
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Results.Unauthorized();
            
            if (roles.IsAnyMatchInvariant("admin", "moderator", "worker")) 
                return await GetAmw(sender, articleId, viewCount, page, sortBy, currencyId, userId, token);
            if (roles.IsAnyMatchInvariant("member"))
                return await GetMember(sender, articleId, viewCount, page, sortBy, currencyId, userId, token);
            
            return Results.Unauthorized();
        })
        .WithGroup("Articles")
        .WithDescription("Получение кросс номеров по id артикула")
        .WithDisplayName("Поиск по кросс номерам");
    }

    private async Task<IResult> GetAmw(ISender sender, int articleId, int viewCount, int page, string? sortBy, int? currencyId, string userId, CancellationToken token)
    {
        var query = new GetArticleCrossesAmwQuery(articleId, viewCount, page, sortBy, currencyId, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleCrossesAmwResponse>();
        return Results.Ok(response);
    }
    
    private async Task<IResult> GetMember(ISender sender, int articleId, int viewCount, int page, string? sortBy, int? currencyId, string userId, CancellationToken token)
    {
        var query = new GetArticleCrossesMemberQuery(articleId, viewCount, page, sortBy, currencyId, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleCrossesMemberResponse>();
        return Results.Ok(response);
    }
}