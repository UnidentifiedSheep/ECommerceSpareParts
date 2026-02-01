using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.StorageOwners.DeleteStorageFromUser;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Users;

public class DeleteStorageFromUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/users/{userId:guid}/storages/{storageName}", 
                async (ISender sender, Guid userId, string storageName, CancellationToken token) =>
                {
                    var command = new DeleteStorageFromUserCommand(userId, storageName);
                    await sender.Send(command, token);
                    return Results.NoContent();
                }).WithTags("Users")
            .WithDescription("Удаление склада у пользователя")
            .WithDisplayName("Удаление склада у пользователя")
            .RequireAnyPermission(PermissionCodes.USERS_STORAGES_DELETE);
    }
}