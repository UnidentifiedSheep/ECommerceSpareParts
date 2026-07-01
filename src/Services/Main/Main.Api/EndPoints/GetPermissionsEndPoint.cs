using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Auth;
using Main.Application.Handlers.Auth.GetPermissions;
using MediatR;

namespace Main.Api.EndPoints;

public record GetPermissionsResponse(IReadOnlyList<PermissionDto> Permissions);

public class GetPermissionsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/permissions/",
                async (
                    ISender sender,
                    CancellationToken ct) =>
                {
                    var query = new GetPermissionsQuery();
                    var result = await sender.Send(query, ct);
                    var response = new GetPermissionsResponse(result.Permissions);
                    return Results.Ok(response);
                })
            .WithTags("Permissions")
            .WithName("GetPermissions")
            .WithSummary("Получить разрешения")
            .WithDescription("Получение разрешений")
            .WithDisplayName("Получение разрешений")
            .Produces<GetPermissionsResponse>()
            .ProducesProblem(400)
            .RequireAnyPermission(PermissionCodes.PERMISSIONS_GET);
    }
}