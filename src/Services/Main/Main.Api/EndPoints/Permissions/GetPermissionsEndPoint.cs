using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Permissions.GetPermission;
using Main.Abstractions.Dtos.Amw.Permissions;
using MediatR;

namespace Main.Api.EndPoints.Permissions;

public record GetPermissionsResponse(IEnumerable<PermissionDto> Permissions);

public class GetPermissionsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/permissions/", async (ISender sender, int page, int limit, CancellationToken ct) =>
        {
            var query = new GetPermissionsQuery(new PaginationModel(page, limit));
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