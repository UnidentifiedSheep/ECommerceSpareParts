using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Application.Handlers.StorageRoutes.GetStorageRouteById;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.StorageRoutes;

public record GetStorageRouteResponse(StorageRouteDto StorageRoute);

public class GetStorageRouteEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/storages/routes/{id:guid}", async (ISender sender, Guid id, CancellationToken token) =>
        {
            var result = await sender.Send(new GetStorageRouteByIdQuery(id), token);
            return Results.Ok(new GetStorageRouteResponse(result.StorageRoute));
        }).WithTags("Storage Routes")
        .WithDescription("Получение маршрута")
        .WithDisplayName("Получение маршрута")
        .RequireAnyPermission(PermissionCodes.STORAGE_ROUTES_GET);
    }
}