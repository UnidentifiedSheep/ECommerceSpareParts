using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.StorageOwners.AddStorageToUser;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Users;

public class AddStorageToUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{userId:guid}/storages/{storageName}", 
            async (ISender sender, Guid userId, string storageName, CancellationToken token) =>
            {
                var command = new AddStorageToUserCommand(userId, storageName);
                await sender.Send(command, token);
                return Results.NoContent();
            }).WithTags("Users")
            .WithDescription("Добавление склада к пользователю")
            .WithDisplayName("Добавление склада к пользователю")
            .RequireAnyPermission(PermissionCodes.USERS_STORAGES_ADD);
    }
}