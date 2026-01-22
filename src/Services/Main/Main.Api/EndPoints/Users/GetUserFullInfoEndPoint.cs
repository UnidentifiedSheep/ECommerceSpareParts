using Api.Common.Extensions;
using Carter;
using Main.Abstractions.Dtos.Amw.Permissions;
using Main.Abstractions.Dtos.Amw.Users;
using Main.Abstractions.Dtos.Roles;
using Main.Abstractions.Dtos.Users;
using Main.Application.Handlers.Users.GetUserFullInfo;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record GetUserFullInfoResponse(UserInfoDto? UserInfo, List<FullEmailDto> Emails, List<RoleDto> Roles, 
    List<PermissionDto> Permissions);

public class GetUserFullInfoEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:guid}/info", async (ISender sender, Guid id, CancellationToken token) =>
            {
                var info = await sender.Send(new GetUserFullInfoQuery(id), token);
                return Results.Ok(new GetUserFullInfoResponse(info.UserInfo, info.Emails, info.Roles, info.Permissions));
            }).WithTags("Users")
            .WithDescription("Получение информации пользователя")
            .WithDisplayName("Получение информации пользователя")
            .RequireAnyPermission(PermissionCodes.USERS_INFO_GET);
    }
}