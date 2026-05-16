using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Main.Application.Dtos.Auth;
using Main.Application.Handlers.Auth.GetRoles;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Roles;

public record GetRolesResponse(IReadOnlyList<RoleDto> Roles);

public record GetRolesRequest : SortablePaginationQueryModel
{
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
}

public class GetRoles : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/roles",
                async (
                    ISender sender,
                    [AsParameters] GetRolesRequest queryParams,
                    CancellationToken cancellationToken) =>
                {
                    var command = new GetRolesQuery(queryParams.SearchTerm, queryParams);
                    var result = await sender.Send(command, cancellationToken);
                    var response = new GetRolesResponse(result.Roles);
                    return Results.Ok(response);
                }).WithTags("Roles")
            .WithDescription("Получение ролей")
            .WithDisplayName("Получение ролей")
            .RequireAnyPermission("ROLES.GET");
    }
}