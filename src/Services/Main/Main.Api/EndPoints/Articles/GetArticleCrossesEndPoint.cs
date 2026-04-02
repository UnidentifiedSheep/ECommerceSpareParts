using Abstractions.Interfaces;
using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.Articles.GetArticleCrosses;
using Mapster;
using MediatR;
using AmwArticleDto = Main.Abstractions.Dtos.Amw.Articles.ArticleFullDto;

namespace Main.Api.EndPoints.Articles;

public record GetArticleCrossesAmwResponse(IEnumerable<AmwArticleDto> Crosses, AmwArticleDto RequestedArticle);

public class GetArticleCrossesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{articleId}/crosses/", async (
                ISender sender,
                IUserContext user,
                int articleId,
                int limit,
                int page,
                string? sortBy,
                CancellationToken token) =>
            {
                var userId = user.UserId;

                var query = new GetArticleCrossesQuery(articleId, new PaginationModel(page, limit), sortBy, userId);
                var result = await sender.Send(query, token);
                var response = result.Adapt<GetArticleCrossesAmwResponse>();
                return Results.Ok(response);
            })
            .RequireAllPermissions(PermissionCodes.ARTICLE_CROSSES_GET)
            .WithTags("Articles")
            .WithDescription("Получение кросс номеров по id артикула")
            .WithDisplayName("Поиск по кросс номерам");
    }
}