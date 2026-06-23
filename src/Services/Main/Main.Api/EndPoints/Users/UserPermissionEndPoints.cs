using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Enums;
using Main.Application.Handlers.Auth;
using Main.Application.Handlers.Auth.AddPermissionToUser;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record AddPermissionToUserRequest
{
    [JsonPropertyName("permission")]
    public required string Permission { get; init; }
}

public static class UserPermissionEndPoints
{
    public static RouteGroupBuilder MapUserPermissionEndPoints(this RouteGroupBuilder users)
    {
        users.MapPost("/{userId:guid}/permissions/", async (
                ISender sender,
                Guid userId,
                AddPermissionToUserRequest request,
                CancellationToken ct) =>
            {
                await sender.Send(new AddPermissionToUserCommand(userId, request.Permission), ct);
                return Results.NoContent();
            })
            .WithName("AddPermissionToUser")
            .WithSummary("Добавить разрешение пользователю")
            .WithDescription("Добавление пользователю разрешение")
            .WithDisplayName("Добавление пользователю разрешение'")
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .RequireAnyPermission(PermissionCodes.USERS_PERMISSIONS_CREATE);
        
        users.MapDelete("/{userId:guid}/permissions/{permission}", async (
                ISender sender,
                Guid userId,
                string permission,
                CancellationToken ct) =>
            {
                await sender.Send(new RemovePermissionFromUserCommand(userId, permission), ct);
                return Results.Ok();
            })
            .WithName("RemovePermissionFromUser")
            .WithSummary("Удаление разрешения у пользователя")
            .WithDescription("Удаление разрешения у пользователя")
            .WithDisplayName("Удаление разрешения у пользователя")
            .Produces(200)
            .ProducesProblem(404)
            .RequireAnyPermission(PermissionCodes.USERS_PERMISSIONS_CREATE);
        
        return users;
    }
}