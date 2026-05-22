using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductWeight.DeleteProductWeight;
using Main.Application.Handlers.ProductWeight.GetProductWeight;
using Main.Application.Handlers.ProductWeight.SetProductWeight;
using MediatR;

namespace Main.Api.EndPoints.ProductWeight;

public record GetProductWeightResponse(ProductWeightDto ProductWeight);

public record PutProductWeightRequest(decimal Weight, WeightUnit Unit);

public class ProductWeightEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var weights = app.MapGroup("/products/{id:int}/weights")
            .WithTags("Product Weight");

        weights.MapDelete("", async (ISender sender, int id, CancellationToken token) =>
            {
                await sender.Send(new DeleteProductWeightCommand(id), token);
                return Results.NoContent();
            })
            .WithDescription("Удаление веса артикула.")
            .WithDisplayName("Удаление веса артикула.")
            .RequireAnyPermission(PermissionCodes.ARTICLE_WEIGHT_DELETE);

        weights.MapGet("", async (ISender sender, int id, CancellationToken token) =>
            {
                var result = await sender.Send(new GetProductWeightQuery(id), token);
                return Results.Ok(new GetProductWeightResponse(result.ProductWeight));
            })
            .WithDescription("Установка веса артикула.")
            .WithDisplayName("Установка веса артикула.")
            .RequireAnyPermission(PermissionCodes.ARTICLE_WEIGHT_GET);

        weights.MapPut("", async (
                ISender sender,
                int id,
                PutProductWeightRequest request,
                CancellationToken token) =>
            {
                await sender.Send(new SetProductWeightCommand(id, request.Weight, request.Unit), token);
                return Results.Created();
            })
            .WithDescription("Установка веса артикула.")
            .WithDisplayName("Установка веса артикула.")
            .RequireAnyPermission(PermissionCodes.ARTICLE_WEIGHT_CREATE);
    }
}
