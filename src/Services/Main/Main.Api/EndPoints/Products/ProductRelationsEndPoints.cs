using Abstractions.Interfaces;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.GetProductCrosses;
using Main.Application.Handlers.Products.GetProductPair;
using Main.Application.Handlers.Products.MakeLinkageBetweenArticles;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Products;

public record GetProductCrossesResponse(IReadOnlyList<ProductDto> Crosses, ProductDto RequestedArticle);

public record GetProductPairResponse(ProductDto? Pair);

public record MakeLinkageBetweenProductsRequest(List<NewProductLinkageDto> Linkages);

public static class ProductRelationsEndPoints
{
    public static RouteGroupBuilder MapProductRelationsEndPoints(this RouteGroupBuilder products)
    {
        products.MapGet("/{productId}/crosses/", async (
                ISender sender,
                IUserContext user,
                int productId,
                [AsParameters] SortablePaginationQueryModel queryParams,
                CancellationToken token) =>
            {
                var query = new GetProductCrossesQuery(productId, queryParams, queryParams.SortBy, user.UserId);
                var result = await sender.Send(query, token);
                var response = new GetProductCrossesResponse(result.Crosses, result.RequestedProduct);
                return Results.Ok(response);
            })
            .RequireAllPermissions(PermissionCodes.ARTICLE_CROSSES_GET)
            .WithDescription("Получение кросс номеров по id артикула")
            .WithDisplayName("Поиск по кросс номерам");

        products.MapGet("/{productId}/pairs", async (
                ISender sender,
                int productId,
                CancellationToken token) =>
            {
                var query = new GetProductPairQuery(productId);
                var result = await sender.Send(query, token);
                return Results.Ok(new GetProductPairResponse(result.Pair));
            })
            .WithDescription("Поиск пар артикула")
            .WithSummary("Поиск пар артикула");

        products.MapPost("/crosses", async (
                ISender sender,
                MakeLinkageBetweenProductsRequest request,
                CancellationToken token) =>
            {
                var command = request.Adapt<MakeLinkageBetweenProductsCommand>();
                await sender.Send(command, token);
                return Results.Created();
            })
            .WithDescription("Создание кроссировки между артикулами")
            .WithDisplayName("Создание кроссировки")
            .Produces(201)
            .ProducesProblem(404)
            .ProducesProblem(400)
            .RequireAnyPermission(PermissionCodes.ARTICLE_CROSSES_CREATE);

        return products;
    }
}
