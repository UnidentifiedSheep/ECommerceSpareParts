using Abstractions.Models;
using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Storage;
using Main.Application.Handlers.StorageOwners.AddStorageToUser;
using Main.Application.Handlers.StorageOwners.DeleteStorageFromUser;
using Main.Application.Handlers.Users.GetUserStorages;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record GetUserStoragesResponse(List<StorageDto> Storages);

public static class UserStorageEndPoints
{
    public static RouteGroupBuilder MapUserStorageEndPoints(this RouteGroupBuilder users)
    {
        users.MapPost(
                "/{userId:guid}/storages/{storageName}",
                async (
                    ISender sender,
                    Guid userId,
                    string storageName,
                    CancellationToken token) =>
                {
                    await sender.Send(new AddStorageToUserCommand(userId, storageName), token);
                    return Results.NoContent();
                })
            .WithName("AddStorageToUser")
            .WithSummary("Добавить склад пользователю")
            .WithDescription("Добавление склада к пользователю")
            .WithDisplayName("Добавление склада к пользователю")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.USERS_STORAGES_ADD);

        users.MapDelete(
                "/{userId:guid}/storages/{storageName}",
                async (
                    ISender sender,
                    Guid userId,
                    string storageName,
                    CancellationToken token) =>
                {
                    await sender.Send(new DeleteStorageFromUserCommand(userId, storageName), token);
                    return Results.NoContent();
                })
            .WithName("DeleteStorageFromUser")
            .WithSummary("Удалить склад пользователя")
            .WithDescription("Удаление склада у пользователя")
            .WithDisplayName("Удаление склада у пользователя")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.USERS_STORAGES_DELETE);

        users.MapGet(
                "/{userId:guid}/storages",
                async (
                    ISender sender,
                    Guid userId,
                    int page,
                    int limit,
                    CancellationToken token) =>
                {
                    var query = new GetUserStoragesQuery(userId, new Pagination(page, limit));
                    var result = await sender.Send(query, token);
                    return Results.Ok(new GetUserStoragesResponse(result.Storages));
                })
            .WithName("GetUserStorages")
            .WithSummary("Получить склады пользователя")
            .WithDescription("Получение складов привязанных к пользователю.")
            .WithDisplayName("Получение складов пользователя.")
            .Produces<GetUserStoragesResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.USERS_STORAGES_GET);

        return users;
    }
}