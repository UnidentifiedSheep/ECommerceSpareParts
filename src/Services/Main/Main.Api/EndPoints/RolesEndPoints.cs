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

namespace Main.Api.EndPoints;

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
            .WithName("AddPermissionToRole")
            .WithSummary("Добавить разрешение роли")
            .WithDescription("Добавление разрешения в роль")
            .WithDisplayName("Добавление разрешения в роль")
            .Accepts<AddPermissionToRoleRequest>(false, "application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(404)
            .RequireAnyPermission(PermissionCodes.ROLES_PERMISSIONS_CREATE);

        roles.MapPost("", async (ISender sender, CreateRoleRequest request, CancellationToken cancellationToken) =>
            {
                await sender.Send(new UpsertRoleCommand(request.Name, request.Description), cancellationToken);
                return Results.Created();
            })
            .WithName("CreateRole")
            .WithSummary("Создать роль")
            .WithDescription("Создание роли")
            .WithDisplayName("Создание роли")
            .Accepts<CreateRoleRequest>(false, "application/json")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.ROLES_CREATE);

        roles.MapGet("", async (
                ISender sender,
                [AsParameters] GetRolesRequest queryParams,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new GetRolesQuery(queryParams.SearchTerm, queryParams), cancellationToken);
                return Results.Ok(new GetRolesResponse(result.Roles));
            })
            .WithName("GetRoles")
            .WithSummary("Получить роли")
            .WithDescription("Получение ролей")
            .WithDisplayName("Получение ролей")
            .Produces<GetRolesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.ROLES_GET);
    }
}
