using Api.Common.Extensions;
using Carter;
using Enums;
using Enums.Units;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductWeight;
using Main.Application.Handlers.ProductWeight.GetProductWeight;
using Main.Application.Handlers.ProductWeight.SetProductWeight;
using MediatR;

namespace Main.Api.EndPoints;

public record GetProductWeightResponse(ProductWeightDto ProductWeight);

public record PutProductWeightRequest(decimal Weight, WeightUnit Unit);

public class ProductWeightEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var weights = app.MapGroup("/products/{id:int}/weights")
            .WithTags("Product Weight");

        weights.MapDelete(
                "",
                async (
                    ISender sender,
                    int id,
                    CancellationToken token) =>
                {
                    await sender.Send(new DeleteProductWeightCommand(id), token);
                    return Results.NoContent();
                })
            .WithName("DeleteProductWeight")
            .WithSummary("Удалить вес продукта")
            .WithDescription("Удаление веса артикула.")
            .WithDisplayName("Удаление веса артикула.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ARTICLE_WEIGHT_DELETE);

        weights.MapGet(
                "",
                async (
                    ISender sender,
                    int id,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(new GetProductWeightQuery(id), token);
                    return Results.Ok(new GetProductWeightResponse(result.ProductWeight));
                })
            .WithName("GetProductWeight")
            .WithSummary("Получить вес продукта")
            .WithDescription("Установка веса артикула.")
            .WithDisplayName("Установка веса артикула.")
            .Produces<GetProductWeightResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ARTICLE_WEIGHT_GET);

        weights.MapPut(
                "",
                async (
                    ISender sender,
                    int id,
                    PutProductWeightRequest request,
                    CancellationToken token) =>
                {
                    await sender.Send(
                        new SetProductWeightCommand(
                            id,
                            request.Weight,
                            request.Unit),
                        token);
                    return Results.Created();
                })
            .WithName("SetProductWeight")
            .WithSummary("Установить вес продукта")
            .WithDescription("Установка веса артикула.")
            .WithDisplayName("Установка веса артикула.")
            .Accepts<PutProductWeightRequest>(false, "application/json")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.ARTICLE_WEIGHT_CREATE);
    }
}