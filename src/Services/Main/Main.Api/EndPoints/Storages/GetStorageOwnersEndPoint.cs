using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.StorageOwners.GetStorageOwners;
using MediatR;

namespace Main.Api.EndPoints.Storages;

public record GetStorageOwnersResponse(IReadOnlyList<UserDto> Owners);

public class GetStorageOwnersEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/storages/{storageName}/owners",
                async (string storageName, int page, int size, ISender sender, CancellationToken cancellationToken) =>
                {
                    var query = new GetStorageOwnersQuery(storageName, new PaginationModel(page, size));
                    var result = await sender.Send(query, cancellationToken);
                    return Results.Ok(new GetStorageOwnersResponse(result.Owners));
                }).RequireAllPermissions(PermissionCodes.USERS_STORAGES_GET)
            .WithName("GetStorageOwners")
            .WithDescription("Получение владельцев склада.")
            .ProducesProblem(404)
            .Produces<GetStorageOwnersResponse>();
    }
}