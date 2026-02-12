using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Abstractions.Dtos.ArticleSizes;
using Main.Application.Handlers.ArticleSizes.GetArticleSizes;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.ArticleSize;

public record GetArticleSizeResponse(ArticleSizeDto ArticleSize);

public class GetArticleSizeEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{id:int}/sizes", async (ISender sender, int id, CancellationToken token) =>
        {
            var result = await sender.Send(new GetArticleSizeQuery(id), token);
            var response = new GetArticleSizeResponse(result.ArticleSize);
            return Results.Ok(response);
        }).WithTags("Article Size")
        .WithDescription("Получение размеров артикула.")
        .WithDisplayName("Получение размеров артикула.")
        .RequireAnyPermission(PermissionCodes.ARTICLE_SIZES_GET);
    }
}