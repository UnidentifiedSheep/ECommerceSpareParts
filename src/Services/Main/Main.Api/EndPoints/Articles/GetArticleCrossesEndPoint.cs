using System.Security.Claims;
using Abstractions.Interfaces;
using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.Articles.GetArticleCrosses;
using Main.Enums;
using Mapster;
using MediatR;
using AmwArticleDto = Main.Abstractions.Dtos.Amw.Articles.ArticleFullDto;
using MemberArticleDto = Main.Abstractions.Dtos.Member.Articles.ArticleFullDto;

namespace Main.Api.EndPoints.Articles;

public record GetArticleCrossesAmwResponse(IEnumerable<AmwArticleDto> Crosses, AmwArticleDto RequestedArticle);

public record GetArticleCrossesMemberResponse(IEnumerable<MemberArticleDto> Crosses, MemberArticleDto RequestedArticle);

public class GetArticleCrossesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{articleId}/crosses/", async (ISender sender, IUserContext user, int articleId,
                int limit, int page, string? sortBy, CancellationToken token) =>
            {
                var userId = user.UserId;
                if (userId == null) return Results.Unauthorized();
                if (!user.ContainsPermission(PermissionCodes.ARTICLE_CROSSES_GET)) return Results.Forbid();

                var pagination = new PaginationModel(page, limit);
                if (user.ContainsPermission(nameof(PermissionCodes.ARTICLES_GET_FULL)))
                    return await GetAmw(sender, articleId, pagination, sortBy, userId, token);
                if (user.ContainsPermission(PermissionCodes.ARTICLES_GET_MAIN))
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