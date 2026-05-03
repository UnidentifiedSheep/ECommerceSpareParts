using Abstractions.Models;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Carter;
using Main.Application.Dtos.Auth;
using Main.Application.Handlers.Auth.GetPermission;
using MediatR;

namespace Main.Api.EndPoints.Permissions;

public record GetPermissionsResponse(IReadOnlyList<PermissionDto> Permissions);

public class GetPermissionsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/permissions/", async (ISender sender, PaginationQueryModel queryParams, CancellationToken ct) =>
            {
                var query = new GetPermissionsQuery(queryParams);
                var result = await sender.Send(query, ct);
                var response = new GetPermissionsResponse(result.Permissions);
                return Results.Ok(response);
            }).WithTags("Permissions")
            .WithDescription("Получение разрешений")
            .WithDisplayName("Получение разрешений")
            .Produces<GetPermissionsResponse>()
            .ProducesProblem(400)
            .RequireAnyPermission("PERMISSIONS.GET");
    }
}