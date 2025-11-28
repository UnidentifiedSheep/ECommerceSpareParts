using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Users.AddPermissionToUser;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record AddPermissionToUserRequest(string Permission);

public class AddPermissionToUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{userId:guid}/permissions/", async (ISender sender, Guid userId,
            AddPermissionToUserRequest request, CancellationToken ct) =>
        {
            var command = new AddPermissionToUserCommand(userId, request.Permission);
            await sender.Send(command, ct);
            return Results.NoContent();
        }).WithTags("User")
        .WithDescription("Добавление пользователю разрешение")
        .WithDisplayName("Добавление пользователю разрешение'")
        .Produces(204)
        .ProducesProblem(400)
        .ProducesProblem(404)
        .RequireAnyPermission("USERS.PERMISSIONS.CREATE");
    }
}