using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.StorageRoutes.DeleteStorageRoute;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.StorageRoutes;

public class DeleteStorageRouteEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/storages/routes/{id:guid}", async (ISender sender, Guid id, CancellationToken token) =>
        {
            var command = new DeleteStorageRouteCommand(id);
            await sender.Send(command, token);
            return Results.NoContent();
        }).WithTags("Storage Routes")
        .WithDescription("Удаление маршрута")
        .WithDisplayName("Удаление маршрута")
        .RequireAnyPermission(PermissionCodes.STORAGE_ROUTES_DELETE);
    }
}