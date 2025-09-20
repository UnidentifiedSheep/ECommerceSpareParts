using System.Security.Claims;
using Application.Handlers.Articles.GetArticleCrosses;
using Carter;
using Core.Models;
using Core.StaticFunctions;
using Mapster;
using MediatR;
using Security.Extensions;
using AmwArticleDto = Core.Dtos.Amw.Articles.ArticleFullDto;
using MemberArticleDto = Core.Dtos.Member.Articles.ArticleFullDto;

namespace MonoliteUnicorn.EndPoints.Articles;

public record GetArticleCrossesAmwResponse(IEnumerable<AmwArticleDto> Crosses, AmwArticleDto RequestedArticle);

public record GetArticleCrossesMemberResponse(IEnumerable<MemberArticleDto> Crosses, MemberArticleDto RequestedArticle);

public class GetArticleCrossesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{articleId}/crosses/", async (ISender sender, ClaimsPrincipal user, int articleId,
                int limit, int page, string? sortBy, CancellationToken token) =>
            {
                var roles = user.GetUserRoles();
                var userId = user.GetUserId();
                if (userId == null) return Results.Unauthorized();

                var pagination = new PaginationModel(page, limit);
                if (roles.IsAnyMatchInvariant("admin", "moderator", "worker"))
                    return await GetAmw(sender, articleId, pagination, sortBy, userId, token);
                if (roles.IsAnyMatchInvariant("member"))
                    return await GetMember(sender, articleId, pagination, sortBy, userId, token);

                return Results.Unauthorized();
            })
            .WithTags("Articles")
            .WithDescription("Получение кросс номеров по id артикула")
            .WithDisplayName("Поиск по кросс номерам");
    }

    private async Task<IResult> GetAmw(ISender sender, int articleId, PaginationModel pagination, string? sortBy,
        string userId, CancellationToken token)
    {
        var query = new GetArticleCrossesQuery<AmwArticleDto>(articleId, pagination, sortBy, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleCrossesAmwResponse>();
        return Results.Ok(response);
    }

    private async Task<IResult> GetMember(ISender sender, int articleId, PaginationModel pagination, string? sortBy,
        string userId, CancellationToken token)
    {
        var query = new GetArticleCrossesQuery<MemberArticleDto>(articleId, pagination, sortBy, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleCrossesMemberResponse>();
        return Results.Ok(response);
    }
}