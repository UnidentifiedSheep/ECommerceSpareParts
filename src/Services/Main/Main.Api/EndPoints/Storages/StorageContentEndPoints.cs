using Abstractions.Models;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Enums;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Application.Handlers.StorageContents.EditContent;
using Main.Application.Handlers.StorageContents.GetContents;
using Main.Application.Handlers.StorageContents.SetToZeroContent;
using Main.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Storages;

public record AddContentToStorageRequest(
    IEnumerable<NewStorageContentDto> StorageContent,
    string StorageName
);

public record EditStorageContentRequest(
    Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>> EditedFields
);

public record GetStorageContentRequest(
    [FromQuery(Name = "storageName")]
    string? StorageName,
    [FromQuery(Name = "productId")]
    int? ArticleId,
    [FromQuery(Name = "showZeroContent")]
    bool ShowZeroCount = true
)
    : PaginationQueryModel;

public record GetStorageContentResponse(IEnumerable<StorageContentDto> Content);

public static class StorageContentEndPoints
{
    public static RouteGroupBuilder MapStorageContentEndPoints(this RouteGroupBuilder storages)
    {
        storages.MapPost(
                "/contents",
                async (
                    ISender sender,
                    AddContentToStorageRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var command = new AddContentCommand(
                        request.StorageContent,
                        request.StorageName,
                        StorageMovementType.StorageContentAddition);
                    await sender.Send(command, cancellationToken);
                    return Results.NoContent();
                })
            .WithName("AddStorageContent")
            .WithSummary("Добавить содержимое склада")
            .WithDescription("Добавление позиций на склад")
            .WithDisplayName("Добавление позиций на склад")
            .Accepts<AddContentToStorageRequest>(false, "application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.STORAGES_CONTENT_CREATE);

        storages.MapDelete(
                "/contents/{contentId:int}",
                async (
                    ISender sender,
                    int contentId,
                    uint rowVersion,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(new SetToZeroContentCommand(contentId, rowVersion), cancellationToken);
                    return Results.NoContent();
                })
            .WithName("DeleteStorageContent")
            .WithSummary("Удалить содержимое склада")
            .WithDescription("Полное удаление позиции со склада по его Id")
            .WithDisplayName("Удаление позиции со склада")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.STORAGES_CONTENT_DELETE);

        storages.MapPatch(
                "/contents",
                async (
                    ISender sender,
                    EditStorageContentRequest request,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(new EditStorageContentCommand(request.EditedFields), cancellationToken);
                    return Results.NoContent();
                })
            .WithName("EditStorageContent")
            .WithSummary("Редактировать содержимое склада")
            .WithDescription("Редактирование позиций на складе")
            .WithDisplayName("Редактирование позиций склада")
            .Accepts<EditStorageContentRequest>(false, "application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.STORAGES_CONTENT_EDIT);

        storages.MapGet(
                "/contents",
                async (
                    ISender sender,
                    CancellationToken token,
                    [AsParameters] GetStorageContentRequest request) =>
                {
                    var query = new GetStorageContentQuery(
                        request.StorageName,
                        request.ArticleId,
                        request,
                        request.ShowZeroCount);
                    var result = await sender.Send(query, token);
                    return Results.Ok(new GetStorageContentResponse(result.Content));
                })
            .WithName("GetStorageContent")
            .WithSummary("Получить содержимое склада")
            .WithDescription("Получение позиций на складе")
            .WithDisplayName("Получение позиций склада")
            .Produces<GetStorageContentResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.STORAGES_CONTENT_GET_ALL);

        return storages;
    }
}