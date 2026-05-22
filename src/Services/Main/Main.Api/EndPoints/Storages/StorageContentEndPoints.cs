using Abstractions.Models;
using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.StorageContents.AddContent;
using Main.Application.Handlers.StorageContents.EditContent;
using Main.Application.Handlers.StorageContents.GetContents;
using Main.Application.Handlers.StorageContents.SetToZeroContent;
using Main.Enums;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Storages;

public record AddContentToStorageRequest(IEnumerable<NewStorageContentDto> StorageContent, string StorageName);

public record EditStorageContentRequest(Dictionary<int, ModelWithRowVersion<PatchStorageContentDto, uint>> EditedFields);

public record GetStorageContentRequest(
    [FromQuery(Name = "storageName")] string? StorageName,
    [FromQuery(Name = "articleId")] int? ArticleId,
    [FromQuery(Name = "page")] int Page,
    [FromQuery(Name = "limit")] int Limit,
    [FromQuery(Name = "showZeroContent")] bool ShowZeroCount = true);

public record GetStorageContentResponse(IEnumerable<StorageContentDto> Content);

public static class StorageContentEndPoints
{
    public static RouteGroupBuilder MapStorageContentEndPoints(this RouteGroupBuilder storages)
    {
        storages.MapPost("/content", async (
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
            .WithDescription("Добавление позиций на склад")
            .WithDisplayName("Добавление позиций на склад")
            .RequireAnyPermission(PermissionCodes.STORAGES_CONTENT_CREATE);

        storages.MapDelete("/content/{contentId}", async (
                ISender sender,
                int contentId,
                uint rowVersion,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new SetToZeroContentCommand(contentId, rowVersion), cancellationToken);
                return Results.NoContent();
            })
            .WithDescription("Полное удаление позиции со склада по его Id")
            .WithDisplayName("Удаление позиции со склада")
            .RequireAnyPermission(PermissionCodes.STORAGES_CONTENT_DELETE);

        storages.MapPatch("/content", async (
                ISender sender,
                EditStorageContentRequest request,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new EditStorageContentCommand(request.EditedFields), cancellationToken);
                return Results.NoContent();
            })
            .WithDescription("Редактирование позиций на складе")
            .WithDisplayName("Редактирование позиций склада")
            .RequireAnyPermission(PermissionCodes.STORAGES_CONTENT_EDIT);

        storages.MapGet("/content", async (
                ISender sender,
                CancellationToken token,
                [AsParameters] GetStorageContentRequest request) =>
            {
                var query = new GetStorageContentQuery(
                    request.StorageName,
                    request.ArticleId,
                    new Pagination(request.Page, request.Limit),
                    request.ShowZeroCount);
                var result = await sender.Send(query, token);
                return Results.Ok(result.Adapt<GetStorageContentResponse>());
            })
            .WithDescription("Получение позиций на складе")
            .WithDisplayName("Получение позиций склада")
            .RequireAnyPermission(PermissionCodes.STORAGES_CONTENT_GET_ALL);

        return storages;
    }
}
