using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Enums;
using Main.Application.Handlers.Auth;
using Main.Application.Handlers.Auth.AddRoleToUser;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record AddRoleToUserRequest
{
    [JsonPropertyName("role")]
    public required string RoleName { get; init; }
}

public static class UserRoleEndPoints
{
    public static RouteGroupBuilder MapUserRoleEndPoints(this RouteGroupBuilder users)
    {
        users.MapPost("/{userId:guid}/roles/", async (
                ISender sender,
                Guid userId,
                AddRoleToUserRequest request,
                CancellationToken ct) =>
            {
                await sender.Send(new AddRoleToUserCommand(userId, request.RoleName), ct);
                return Results.NoContent();
            })
            .WithName("AddRoleToUser")
            .WithSummary("Добавить роли пользователю")
            .WithDescription("Добавление пользователю роли")
            .WithDisplayName("Добавление пользователю роли'")
            .Accepts<AddRoleToUserRequest>("application/json")
            .Produces(204)
            .ProducesProblem(400)
            .ProducesProblem(404)
            .RequireAnyPermission(PermissionCodes.USERS_ROLES_CREATE);
        
        return users;
    }
}