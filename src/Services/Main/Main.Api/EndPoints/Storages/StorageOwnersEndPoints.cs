using Abstractions.Models;
using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.StorageOwners.GetStorageOwners;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public record GetStorageOwnersResponse(IReadOnlyList<UserDto> Owners);

public static class StorageOwnersEndPoints
{
    public static RouteGroupBuilder MapStorageOwnersEndPoints(this RouteGroupBuilder storages)
    {
        storages.MapGet("/{storageName}/owners", async (
                string storageName,
                int page,
                int size,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new GetStorageOwnersQuery(storageName, new Pagination(page, size));
                var result = await sender.Send(query, cancellationToken);
                return Results.Ok(new GetStorageOwnersResponse(result.Owners));
            })
            .RequireAllPermissions(PermissionCodes.USERS_STORAGES_GET)
            .WithName("GetStorageOwners")
            .WithSummary("Получить владельцев склада")
            .WithDescription("Получение владельцев склада.")
            .ProducesProblem(404)
            .Produces<GetStorageOwnersResponse>();

        return storages;
    }
}
