using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Permissions.CreatePermission;
using MediatR;

namespace Main.Api.EndPoints.Permissions;

public record CreatePermissionRequest(string Name, string? Description);
public record CreatePermissionResult(string Name);

public class CreatePermissionEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/permissions/", async (ISender sender, CreatePermissionRequest request, CancellationToken ct) =>
        {
            var command = new CreatePermissionCommand(request.Name, request.Description);
            var result = await sender.Send(command, ct);
            var response = new CreatePermissionResult(result.Name);
            return Results.Created($"/permissions/{response.Name}", response);
        }).WithTags("Permissions")
        .WithDescription("Создание 'разрешения'")
        .WithDisplayName("Создание 'разрешения'")
        .Produces<CreatePermissionResult>(201)
        .ProducesProblem(400)
        .ProducesProblem(404)
        .RequireAnyPermission("PERMISSIONS.CREATE");
    }
}