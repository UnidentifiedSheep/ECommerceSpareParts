using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Users.EditUserInfo;
using Main.Application.Handlers.Users.GetUserFullInfo;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record GetUserDiscountResponse(decimal Discount);

public record GetUserFullInfoResponse(
    UserDto User,
    IReadOnlyList<UserEmailDto> Emails,
    IReadOnlyList<UserPhoneDto> Phones,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions
);

public record EditUserInfoRequest
{
    [JsonPropertyName("userInfo")]
    public required UserInfoDto UserInfo { get; init; }
}

public record EditUserInfoResponse
{
    [JsonPropertyName("userInfo")]
    public required UserInfoDto UserInfo { get; init; }
}

public static class UserInfoEndPoints
{
    public static RouteGroupBuilder MapUserInfoEndPoints(this RouteGroupBuilder users)
    {
        users.MapGet(
                "/{id:guid}/info",
                async (
                    ISender sender,
                    Guid id,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(new GetUserFullInfoQuery(id), token);
                    return Results.Ok(
                        new GetUserFullInfoResponse(
                            result.User,
                            result.Emails,
                            result.Phones,
                            result.Roles,
                            result.Permissions));
                })
            .WithName("GetUserFullInfo")
            .WithSummary("Получить полную информацию пользователя")
            .WithDescription("Получение информации пользователя")
            .WithDisplayName("Получение информации пользователя")
            .Produces<GetUserFullInfoResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.USERS_INFO_GET);

        users.MapPut(
                "/{id:guid}/info",
                async (
                    ISender sender,
                    Guid id,
                    EditUserInfoRequest request,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(new EditUserInfoCommand(id, request.UserInfo), token);
                    return Results.Ok(
                        new EditUserInfoResponse
                        {
                            UserInfo = result.UserInfo
                        });
                })
            .WithName("EditUserInfo")
            .WithSummary("Редактирование информации пользователя")
            .WithDescription("Редактирование информации пользователя")
            .WithDisplayName("Редактирование информации пользователя")
            .Produces<EditUserInfoResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.USERS_CREATE);

        return users;
    }
}