using Application.Handlers.Roles.CreateRole;
using Carter;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Roles;

public record CreateRoleRequest(string Name, string? Description);

public class CreateRoleEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/roles", async (ISender sender, CreateRoleRequest request, CancellationToken cancellationToken) =>
        {
            var command = new CreateRoleCommand(request.Name, request.Description);
            await sender.Send(command, cancellationToken);
            return Results.Created();
        }).RequireAuthorization("AM")
        .WithTags("Roles")
        .WithDescription("Создание роли")
        .WithDisplayName("Создание роли");
    }
}