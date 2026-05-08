using Abstractions.Interfaces;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.Products.GetProductCrosses;
using MediatR;

namespace Main.Api.EndPoints.Articles;

public record GetProductCrossesResponse(IReadOnlyList<ProductDto> Crosses, ProductDto RequestedArticle);

public class GetProductCrossesEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{productId}/crosses/", async (
                ISender sender,
                IUserContext user,
                int productId,
                [AsParameters] SortablePaginationQueryModel queryParams,
                CancellationToken token) =>
            {
                var userId = user.UserId;

                var query = new GetProductCrossesQuery(productId, queryParams, queryParams.SortBy, userId);
                var result = await sender.Send(query, token);
                var response = new GetProductCrossesResponse(result.Crosses, result.RequestedProduct);
                return Results.Ok(response);
            })
            .RequireAllPermissions(PermissionCodes.ARTICLE_CROSSES_GET)
            .WithTags("Articles")
            .WithDescription("Получение кросс номеров по id артикула")
            .WithDisplayName("Поиск по кросс номерам");
    }
}