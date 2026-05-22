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

namespace Main.Api.EndPoints.ProductCharacteristics;

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
            .WithName("Создание характеристик артикула")
            .WithDescription("Создание характеристик артикула")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Создание характеристик артикула")
            .RequireAnyPermission(PermissionCodes.ARTICLE_CHARACTERISTICS_CREATE);

        characteristics.MapDelete("/{productId}/characteristics/{name}", async (
                ISender sender,
                int productId,
                string name,
                CancellationToken token) =>
            {
                await sender.Send(new DeleteCharacteristicsCommand(productId, name), token);
                return Results.Ok();
            })
            .WithName("Удаление характеристики артикула по id")
            .WithDescription("Удаление характеристики")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Удаление характеристики")
            .RequireAnyPermission(PermissionCodes.ARTICLE_CHARACTERISTICS_DELETE);

        characteristics.MapPatch("/{productId}/characteristics/{name}", async (
                ISender sender,
                int productId,
                string name,
                EditCharacteristicsRequest request,
                CancellationToken token) =>
            {
                await sender.Send(new PatchCharacteristicsCommand(productId, name, request.Value), token);
                return Results.Ok();
            })
            .WithName("Редактирование характеристики артикула по id")
            .WithDescription("Редактирование характеристики")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Редактирование характеристики")
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
            .WithName("Получение характеристик артикула по id")
            .WithDescription("Получить характеристики артикула")
            .Produces<GetProductCharacteristicsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Получение характеристик артикула по id");
    }
}
