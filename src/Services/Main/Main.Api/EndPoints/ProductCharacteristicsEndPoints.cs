using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductCharacteristics.AddCharacteristics;
using Main.Application.Handlers.ProductCharacteristics.DeleteCharacteristics;
using Main.Application.Handlers.ProductCharacteristics.GetCharacteristics;
using Main.Application.Handlers.ProductCharacteristics.PatchCharacteristics;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints;

public record AddCharacteristicsRequest(IEnumerable<NewCharacteristicsDto> Characteristics);

public record EditCharacteristicsRequest(PatchCharacteristicsDto Value);

public record GetProductCharacteristicsResponse(IReadOnlyList<ProductCharacteristicDto> Characteristics);

public class ProductCharacteristicsEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var characteristics = app.MapGroup("/products")
            .WithTags("Product Characteristics");

        characteristics.MapPost("/characteristics/", async (
                ISender sender,
                AddCharacteristicsRequest request,
                CancellationToken token) =>
            {
                await sender.Send(new AddCharacteristicsCommand(request.Characteristics), token);
                return Results.Created();
            })
            .WithName("CreateProductCharacteristics")
            .WithDescription("Создание характеристик артикула")
            .Accepts<AddCharacteristicsRequest>(false, "application/json")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Создать характеристики продуктов")
            .RequireAnyPermission(PermissionCodes.ARTICLE_CHARACTERISTICS_CREATE);

        characteristics.MapDelete("/{productId:int}/characteristics/{name}", async (
                ISender sender,
                int productId,
                string name,
                CancellationToken token) =>
            {
                await sender.Send(new DeleteCharacteristicsCommand(productId, name), token);
                return Results.Ok();
            })
            .WithName("DeleteProductCharacteristic")
            .WithDescription("Удаление характеристики")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Удалить характеристику продукта")
            .RequireAnyPermission(PermissionCodes.ARTICLE_CHARACTERISTICS_DELETE);

        characteristics.MapPatch("/{productId:int}/characteristics/{name}", async (
                ISender sender,
                int productId,
                string name,
                EditCharacteristicsRequest request,
                CancellationToken token) =>
            {
                await sender.Send(new PatchCharacteristicsCommand(productId, name, request.Value), token);
                return Results.Ok();
            })
            .WithName("EditProductCharacteristic")
            .WithDescription("Редактирование характеристики")
            .Accepts<EditCharacteristicsRequest>(false, "application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Редактировать характеристику продукта")
            .RequireAnyPermission(PermissionCodes.ARTICLE_CHARACTERISTICS_UPDATE);

        characteristics.MapGet("/{productId:int}/characteristics", async (
                ISender sender,
                int productId,
                [AsParameters] PaginationQueryModel queryParams,
                CancellationToken token) =>
            {
                var result = await sender.Send(new GetCharacteristicsQuery(productId, queryParams), token);
                return Results.Ok(result.Adapt<GetProductCharacteristicsResponse>());
            })
            .WithName("GetProductCharacteristics")
            .WithDescription("Получить характеристики артикула")
            .Produces<GetProductCharacteristicsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Получить характеристики продукта");
    }
}
