using System.Text.Json.Serialization;
using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.Storages.CreateStorage;
using Main.Application.Handlers.Storages.DeleteStorage;
using Main.Application.Handlers.Storages.EditStorage;
using Main.Application.Handlers.Storages.GetStorage;
using Main.Application.Handlers.Storages.GetStorageByCode;
using Main.Entities.Exceptions;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public record CreateStorageRequest
{
    [JsonPropertyName("code")]
    public required string Code { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("location")]
    public string? Location { get; init; }

    [JsonPropertyName("type")]
    public StorageType Type { get; init; }
}

public record CreateStorageResponse
{
    [JsonPropertyName("code")]
    public required string Code { get; init; }
}

public record EditStorageRequest
{
    [JsonPropertyName("editStorage")]
    public required PatchStorageDto EditStorage { get; init; }
}

public record GetStoragesResponse
{
    [JsonPropertyName("storages")]
    public required IEnumerable<StorageDto> Storages { get; init; }
}

public record GetStorageByCodeResponse
{
    [JsonPropertyName("storage")]
    public required StorageDto Storage { get; init; }
}

public class StoragesEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var storages = app.MapGroup("/storages")
            .WithTags("Storages");

        storages.MapStorageContentEndPoints();
        storages.MapStorageOwnersEndPoints();

        storages.MapPost(
                "/",
                async (
                    ISender sender,
                    CreateStorageRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new CreateStorageCommand(
                            request.Code,
                            request.Description,
                            request.Location,
                            request.Type),
                        cancellationToken);
                    return Results.Created(
                        "/storages/",
                        new CreateStorageResponse
                        {
                            Code = result.Code
                        });
                })
            .WithName("CreateStorage")
            .WithSummary("Создать склад")
            .WithDescription("Создание нового склада")
            .WithDisplayName("Создать склад")
            .Accepts<CreateStorageRequest>(false, "application/json")
            .Produces<CreateStorageResponse>(201)
            .ProducesProblem(400)
            .RequireAnyPermission(PermissionCodes.STORAGES_CREATE);

        storages.MapDelete(
                "/{storageCode}",
                async (
                    ISender sender,
                    string storageCode,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(new DeleteStorageCommand(storageCode), cancellationToken);
                    return Results.NoContent();
                })
            .WithName("DeleteStorage")
            .WithSummary("Удалить склад")
            .WithDescription("Полное удаление склада по его коду")
            .WithDisplayName("Удаление склада")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.STORAGES_DELETE);

        storages.MapPatch(
                "/{storageCode}",
                async (
                    ISender sender,
                    string storageCode,
                    EditStorageRequest request,
                    CancellationToken token) =>
                {
                    await sender.Send(new EditStorageCommand(storageCode, request.EditStorage), token);
                    return Results.NoContent();
                })
            .WithName("EditStorage")
            .WithSummary("Редактировать склад")
            .WithDescription("Редактирование полей склада")
            .WithDisplayName("Редактирование склада")
            .Accepts<EditStorageRequest>(false, "application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.STORAGES_EDIT);

        storages.MapGet(
                "/",
                async (
                    ISender sender,
                    int page,
                    int limit,
                    string? searchTerm,
                    StorageType? type,
                    CancellationToken token) =>
                {
                    var query = new GetStoragesQuery(
                        new Pagination(page, limit),
                        searchTerm,
                        type);
                    var result = await sender.Send(query, token);
                    return Results.Ok(
                        new GetStoragesResponse
                        {
                            Storages = result.Storages
                        });
                })
            .WithName("GetStorages")
            .WithSummary("Получить склады")
            .WithDescription("Поиск и получение существующих складов")
            .Produces<GetStoragesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDisplayName("Получение складов")
            .RequireAnyPermission(PermissionCodes.STORAGES_GET);

        storages.MapGet(
                "/{code}",
                async (
                    ISender sender,
                    string code,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(new GetStorageByCodeQuery(code), token);
                    return Results.Ok(
                        new GetStorageByCodeResponse
                        {
                            Storage = result.Storage
                        });
                })
            .WithName("GetStorageByCode")
            .WithSummary("Получить склад по коду")
            .WithDescription("Получение склада по коду")
            .WithDisplayName("Получение склада по коду")
            .Produces<GetStorageByCodeResponse>()
            .Produces<StorageNotFoundException>(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.STORAGES_GET);
    }
}
