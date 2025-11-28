using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Roles.AddPermissionToRole;
using MediatR;

namespace Main.Api.EndPoints.Roles;

public record AddPermissionToRoleRequest(string PermissionName);

public class AddPermissionToRoleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/roles/{roleId:guid}/permissions/", async 
            (ISender sender, Guid roleId, AddPermissionToRoleRequest request, CancellationToken ct) =>
        {
            var command = new AddPermissionToRoleCommand(roleId, request.PermissionName);
            await sender.Send(command, ct);
            return Results.NoContent();
        }).WithTags("Roles")
        .WithDescription("Добавление разрешения в роль")
        .WithDisplayName("Добавление разрешения в роль")
        .ProducesProblem(404)
        .Produces(200)
        .RequireAnyPermission("ROLES.PERMISSIONS.CREATE");
    }
}