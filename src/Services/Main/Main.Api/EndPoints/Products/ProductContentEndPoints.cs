using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Product;
using Main.Application.Handlers.ProductContent.AddProductContent;
using Main.Application.Handlers.ProductContent.GetProductContents;
using Main.Application.Handlers.ProductContent.RemoveProductContent;
using Main.Application.Handlers.ProductContent.SetProductContentQuantity;
using MediatR;

namespace Main.Api.EndPoints.Products;

public record AddProductContentRequest(Dictionary<int, int> Content);

public record GetProductContentResponse(IReadOnlyList<ProductContentDto> Content);

public record SetProductsContentCountRequest(int Count);

public static class ProductContentEndPoints
{
    public static RouteGroupBuilder MapProductContentEndPoints(this RouteGroupBuilder products)
    {
        products.MapGet(
                "/{productId:int}/contents",
                async (
                    ISender sender,
                    int productId,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(new GetProductContentsQuery(productId), token);
                    var response = new GetProductContentResponse(result.Contents);
                    return Results.Ok(response);
                })
            .WithName("GetProductContent")
            .WithDescription("Получить содержимое артикула по id.")
            .Produces<GetProductContentResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Получить содержимое продукта");

        products.MapPost(
                "/{productId:int}/contents",
                async (
                    ISender sender,
                    int productId,
                    AddProductContentRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var command = new AddProductContentCommand(productId, request.Content);
                    await sender.Send(command, cancellationToken);
                    return Results.NoContent();
                })
            .WithName("AddProductContent")
            .WithSummary("Добавить содержимое продукта")
            .WithDescription("Добавление содержимого артикула")
            .WithDisplayName("Добавление содержимого артикула")
            .Accepts<AddProductContentRequest>(false, "application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ARTICLE_CONTENT_CREATE);

        products.MapDelete(
                "/{productId:int}/contents/{childProductId:int}",
                async (
                    ISender sender,
                    int productId,
                    int childProductId,
                    CancellationToken token) =>
                {
                    var command = new RemoveProductContentCommand(productId, childProductId);
                    await sender.Send(command, token);
                    return Results.NoContent();
                })
            .WithName("RemoveProductContent")
            .WithSummary("Удалить содержимое продукта")
            .WithDescription("Удаление содержимого артикула в бд")
            .WithDisplayName("Удаление содержимого артикула")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ARTICLE_CONTENT_DELETE);

        products.MapPatch(
                "/{productId:int}/contents/{childProductId:int}",
                async (
                    ISender sender,
                    int productId,
                    int childProductId,
                    SetProductsContentCountRequest request,
                    CancellationToken token) =>
                {
                    var command = new SetProductsContentCountCommand(
                        productId,
                        childProductId,
                        request.Count);
                    await sender.Send(command, token);
                    return Results.NoContent();
                })
            .WithName("SetProductContentCount")
            .WithSummary("Изменить количество содержимого продукта")
            .Accepts<SetProductsContentCountRequest>(false, "application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ARTICLE_CONTENT_EDIT);

        return products;
    }
}