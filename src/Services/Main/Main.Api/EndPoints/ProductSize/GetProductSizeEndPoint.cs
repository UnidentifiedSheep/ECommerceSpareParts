using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductSizes.GetProductSizes;
using MediatR;

namespace Main.Api.EndPoints.ArticleSize;

public record GetProductSizeResponse(ProductSizeDto ProductSize);

public class GetProductSizeEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:int}/sizes", async (ISender sender, int id, CancellationToken token) =>
            {
                var result = await sender.Send(new GetProductSizeQuery(id), token);
                var response = new GetProductSizeResponse(result.ProductSize);
                return Results.Ok(response);
            }).WithTags("Article Size")
            .WithDescription("Получение размеров артикула.")
            .WithDisplayName("Получение размеров артикула.")
            .RequireAnyPermission(PermissionCodes.ARTICLE_SIZES_GET);
    }
}