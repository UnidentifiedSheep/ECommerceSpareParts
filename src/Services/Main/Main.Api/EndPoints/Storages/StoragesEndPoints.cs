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
using Main.Entities.Exceptions.Storages;
using Main.Enums;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public record CreateStorageRequest(string Name, string? Description, string? Location, StorageType Type);

public record CreateStorageResponse(string Name);

public record EditStorageRequest(PatchStorageDto EditStorage);

public record GetStoragesResponse(IEnumerable<StorageDto> Storages);

public record GetStorageByNameResponse(StorageDto Storage);

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
                var result = await sender.Send(request.Adapt<CreateStorageCommand>(), cancellationToken);
                return Results.Created("/storages/", new CreateStorageResponse(result.Name));
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
                return Results.Ok(result.Adapt<GetStoragesResponse>());
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
                return Results.Ok(result.Adapt<GetStorageByNameResponse>());
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
