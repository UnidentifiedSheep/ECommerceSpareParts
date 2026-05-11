using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Users.GetUserFullInfo;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record GetUserFullInfoResponse(
    UserDto User,
    IReadOnlyList<UserEmailDto> Emails,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public class GetUserFullInfoEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:guid}/info", async (ISender sender, Guid id, CancellationToken token) =>
            {
                var result = await sender.Send(new GetUserFullInfoQuery(id), token);
                return Results.Ok(new GetUserFullInfoResponse(
                    result.User,
                    result.Emails,
                    result.Roles,
                    result.Permissions));
            }).WithTags("Users")
            .WithDescription("Получение информации пользователя")
            .WithDisplayName("Получение информации пользователя")
            .RequireAnyPermission(PermissionCodes.USERS_INFO_GET);
    }
}