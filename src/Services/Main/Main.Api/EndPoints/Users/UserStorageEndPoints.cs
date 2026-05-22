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
        users.MapPost("/{userId:guid}/storages/{storageName}", async (
                ISender sender,
                Guid userId,
                string storageName,
                CancellationToken token) =>
            {
                await sender.Send(new AddStorageToUserCommand(userId, storageName), token);
                return Results.NoContent();
            })
            .WithDescription("Добавление склада к пользователю")
            .WithDisplayName("Добавление склада к пользователю")
            .RequireAnyPermission(PermissionCodes.USERS_STORAGES_ADD);

        users.MapDelete("/{userId:guid}/storages/{storageName}", async (
                ISender sender,
                Guid userId,
                string storageName,
                CancellationToken token) =>
            {
                await sender.Send(new DeleteStorageFromUserCommand(userId, storageName), token);
                return Results.NoContent();
            })
            .WithDescription("Удаление склада у пользователя")
            .WithDisplayName("Удаление склада у пользователя")
            .RequireAnyPermission(PermissionCodes.USERS_STORAGES_DELETE);

        users.MapGet("/{userId:guid}/storages", async (
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
            .WithDescription("Получение складов привязанных к пользователю.")
            .WithDisplayName("Получение складов пользователя.")
            .RequireAnyPermission(PermissionCodes.USERS_STORAGES_GET);

        return users;
    }
}
