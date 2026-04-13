using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Abstractions.Dtos.ArticleSizes;
using Main.Application.Handlers.ArticleSizes.GetArticleSizes;
using MediatR;

namespace Main.Api.EndPoints.ArticleSize;

public record GetArticleSizeResponse(ProductSizeDto ProductSize);

public class GetArticleSizeEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/articles/{id:int}/sizes", async (ISender sender, int id, CancellationToken token) =>
            {
                var result = await sender.Send(new GetProductSizeQuery(id), token);
                var response = new GetArticleSizeResponse(result.ProductSize);
                return Results.Ok(response);
            }).WithTags("Article Size")
            .WithDescription("Получение размеров артикула.")
            .WithDisplayName("Получение размеров артикула.")
            .RequireAnyPermission(PermissionCodes.ARTICLE_SIZES_GET);
    }
}