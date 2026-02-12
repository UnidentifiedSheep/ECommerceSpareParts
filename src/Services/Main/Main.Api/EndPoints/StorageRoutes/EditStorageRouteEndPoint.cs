using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Abstractions.Dtos.Amw.StorageRoutes;
using Main.Application.Handlers.StorageRoutes.EditStorageRoute;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.StorageRoutes;

public record EditStorageRouteRequest(PatchStorageRouteDto PatchStorageRoute);

public class EditStorageRouteEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/storages/routes/{id:guid}", async (ISender sender, Guid id, 
            EditStorageRouteRequest  request, CancellationToken cancellationToken) =>
        {
            var command = new EditStorageRouteCommand(id, request.PatchStorageRoute);
            await sender.Send(command, cancellationToken);
            return Results.Ok();
        }).WithTags("Storage Routes")
        .WithDescription("Обновление маршрута")
        .WithDisplayName("Обновление маршрута")
        .RequireAnyPermission(PermissionCodes.STORAGE_ROUTES_EDIT);
    }
}