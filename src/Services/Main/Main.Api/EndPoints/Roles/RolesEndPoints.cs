using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Enums;
using Main.Application.Dtos.Auth;
using Main.Application.Handlers.Auth.AddPermissionToRole;
using Main.Application.Handlers.Auth.GetRoles;
using Main.Application.Handlers.Auth.UpsertRole;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Roles;

public record AddPermissionToRoleRequest(string PermissionName);

public record CreateRoleRequest(string Name, string? Description);

public record GetRolesResponse(IReadOnlyList<RoleDto> Roles);

public record GetRolesRequest : SortablePaginationQueryModel
{
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
}

public class RolesEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var roles = app.MapGroup("/roles")
            .WithTags("Roles");

        roles.MapPost("/{roleName}/permissions/", async (
                ISender sender,
                string roleName,
                AddPermissionToRoleRequest request,
                CancellationToken ct) =>
            {
                await sender.Send(new AddPermissionToRoleCommand(roleName, request.PermissionName), ct);
                return Results.NoContent();
            })
            .WithDescription("Добавление разрешения в роль")
            .WithDisplayName("Добавление разрешения в роль")
            .ProducesProblem(404)
            .Produces(200)
            .RequireAnyPermission(PermissionCodes.ROLES_PERMISSIONS_CREATE);

        roles.MapPost("", async (ISender sender, CreateRoleRequest request, CancellationToken cancellationToken) =>
            {
                await sender.Send(new UpsertRoleCommand(request.Name, request.Description), cancellationToken);
                return Results.Created();
            })
            .WithDescription("Создание роли")
            .WithDisplayName("Создание роли")
            .RequireAnyPermission(PermissionCodes.ROLES_CREATE);

        roles.MapGet("", async (
                ISender sender,
                [AsParameters] GetRolesRequest queryParams,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetRolesQuery(queryParams.SearchTerm, queryParams), cancellationToken);
                return Results.Ok(new GetRolesResponse(result.Roles));
            })
            .WithDescription("Получение ролей")
            .WithDisplayName("Получение ролей")
            .RequireAnyPermission(PermissionCodes.ROLES_GET);
    }
}
