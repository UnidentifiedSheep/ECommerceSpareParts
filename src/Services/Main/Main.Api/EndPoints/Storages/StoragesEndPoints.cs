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
using Main.Application.Handlers.Storages.GetStorageByName;
using Main.Entities.Exceptions;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public record CreateStorageRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    
    [JsonPropertyName("location")]
    public string? Location { get; init; }
    
    [JsonPropertyName("type")]
    public StorageType Type { get; init; }
}

public record CreateStorageResponse
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
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

public record GetStorageByNameResponse
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

        storages.MapPost("/", async (
                ISender sender,
                CreateStorageRequest request,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new CreateStorageCommand(
                        request.Name, 
                        request.Description, 
                        request.Location, 
                        request.Type), 
                    cancellationToken);
                return Results.Created("/storages/", new CreateStorageResponse
                {
                    Name = result.Name
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

        storages.MapDelete("/{storageName}", async (
                ISender sender,
                string storageName,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteStorageCommand(storageName), cancellationToken);
                return Results.NoContent();
            })
            .WithName("DeleteStorage")
            .WithSummary("Удалить склад")
            .WithDescription("Полное удаление склада по его имени")
            .WithDisplayName("Удаление склада")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.STORAGES_DELETE);

        storages.MapPatch("/{storageName}", async (
                ISender sender,
                string storageName,
                EditStorageRequest request,
                CancellationToken token) =>
            {
                await sender.Send(new EditStorageCommand(storageName, request.EditStorage), token);
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

        storages.MapGet("/", async (
                ISender sender,
                int page,
                int limit,
                string? searchTerm,
                StorageType? type,
                CancellationToken token) =>
            {
                var query = new GetStoragesQuery(new Pagination(page, limit), searchTerm, type);
                var result = await sender.Send(query, token);
                return Results.Ok(new GetStoragesResponse
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

        storages.MapGet("/{name}", async (ISender sender, string name, CancellationToken token) =>
            {
                var result = await sender.Send(new GetStorageByNameQuery(name), token);
                return Results.Ok(new GetStorageByNameResponse
                {
                    Storage = result.Storage
                });
            })
            .WithName("GetStorageByName")
            .WithSummary("Получить склад по имени")
            .WithDescription("Получение склада по имени")
            .WithDisplayName("Получение склада по имени")
            .Produces<GetStorageByNameResponse>()
            .Produces<StorageNotFoundException>(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.STORAGES_GET);
    }
}
