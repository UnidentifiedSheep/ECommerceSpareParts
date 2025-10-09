using Carter;
using Core.Dtos.Roles;
using Core.Models;
using Main.Application.Handlers.Roles.GetRoles;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Roles;

public record GetRolesResponse(IEnumerable<RoleDto> Roles);

public class GetRoles : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/roles",
                async (ISender sender, int page, int limit, string? searchTerm, CancellationToken cancellationToken) =>
                {
                    var command = new GetRolesQuery(searchTerm, new PaginationModel(page, limit));
                    var result = await sender.Send(command, cancellationToken);
                    var response = result.Adapt<GetRolesResponse>();
                    return Results.Ok(response);
                }).RequireAuthorization("AMW")
            .WithTags("Roles")
            .WithDescription("Получение ролей")
            .WithDisplayName("Получение ролей");
    }
}