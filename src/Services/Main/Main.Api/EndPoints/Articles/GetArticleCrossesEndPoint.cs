using System.Security.Claims;
using Carter;
using Core.Models;
using Main.Application.Handlers.Articles.GetArticleCrosses;
using Mapster;
using MediatR;
using Security.Extensions;
using AmwArticleDto = Main.Core.Dtos.Amw.Articles.ArticleFullDto;
using MemberArticleDto = Main.Core.Dtos.Member.Articles.ArticleFullDto;

namespace Main.Api.EndPoints.Articles;

public record GetArticleCrossesAmwResponse(IEnumerable<AmwArticleDto> Crosses, AmwArticleDto RequestedArticle);

public record GetArticleCrossesMemberResponse(IEnumerable<MemberArticleDto> Crosses, MemberArticleDto RequestedArticle);

public class GetArticleCrossesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{articleId}/crosses/", async (ISender sender, ClaimsPrincipal user, int articleId,
                int limit, int page, string? sortBy, CancellationToken token) =>
            {
                if (!user.GetUserId(out var userId)) return Results.Unauthorized();
                if (!user.HasPermissions("ARTICLE.CROSSES.GET")) return Results.Forbid();

                var pagination = new PaginationModel(page, limit);
                if (user.HasPermissions("ARTICLES.GET.FULL"))
                    return await GetAmw(sender, articleId, pagination, sortBy, userId, token);
                if (user.HasPermissions("ARTICLES.GET.Main"))
                    return await GetMember(sender, articleId, pagination, sortBy, userId, token);
                return Results.Forbid();
            })
            .WithTags("Articles")
            .WithDescription("Получение кросс номеров по id артикула")
            .WithDisplayName("Поиск по кросс номерам");
    }

    private async Task<IResult> GetAmw(ISender sender, int articleId, PaginationModel pagination, string? sortBy,
        Guid? userId, CancellationToken token)
    {
        var query = new GetArticleCrossesQuery<AmwArticleDto>(articleId, pagination, sortBy, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleCrossesAmwResponse>();
        return Results.Ok(response);
    }

    private async Task<IResult> GetMember(ISender sender, int articleId, PaginationModel pagination, string? sortBy,
        Guid? userId, CancellationToken token)
    {
        var query = new GetArticleCrossesQuery<MemberArticleDto>(articleId, pagination, sortBy, userId);
        var result = await sender.Send(query, token);
        var response = result.Adapt<GetArticleCrossesMemberResponse>();
        return Results.Ok(response);
    }
}