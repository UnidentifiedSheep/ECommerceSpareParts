using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductWeight.GetProductWeight;
using MediatR;

namespace Main.Api.EndPoints.ProductWeight;

public record GetProductWeightResponse(ProductWeightDto ProductWeight);

public class GetProductWeightEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/products/{id:int}/weights", async (ISender sender, int id, CancellationToken token) =>
            {
                var result = await sender.Send(new GetProductWeightQuery(id), token);
                var response = new GetProductWeightResponse(result.ProductWeight);
                return Results.Ok(response);
            }).WithTags("Article Weight")
            .WithDescription("Установка веса артикула.")
            .WithDisplayName("Установка веса артикула.")
            .RequireAnyPermission(PermissionCodes.ARTICLE_WEIGHT_GET);
    }
}