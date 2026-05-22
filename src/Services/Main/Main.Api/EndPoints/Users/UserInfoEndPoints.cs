using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Users;
using Main.Application.Handlers.Users;
using Main.Application.Handlers.Users.ChangeUserDiscount;
using Main.Application.Handlers.Users.GetUserDiscount;
using Main.Application.Handlers.Users.GetUserFullInfo;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record ChangeDiscountForUserRequest(decimal NewDiscountRate);

public record CreateMailForUserRequest(string MailBox, string? Password, string? Comment);

public record CreateMailForUserResponse(string MailBoxAddress, string Password);

public record GetUserDiscountResponse(decimal Discount);

public record GetUserFullInfoResponse(
    UserDto User,
    IReadOnlyList<UserEmailDto> Emails,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public static class UserInfoEndPoints
{
    public static RouteGroupBuilder MapUserInfoEndPoints(this RouteGroupBuilder users)
    {
        users.MapPatch("/{userId}/discount/", async (
                ISender sender,
                Guid userId,
                ChangeDiscountForUserRequest request,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new ChangeUserDiscountCommand(userId, request.NewDiscountRate), cancellationToken);
                return Results.Ok();
            })
            .WithDescription("Изменение скидки пользователя")
            .WithDisplayName("Поменять скидку")
            .RequireAnyPermission(PermissionCodes.USERS_DISCOUNT_CREATE);

        users.MapGet("/{id:guid}/discount", async (ISender sender, Guid id, CancellationToken token) =>
            {
                var result = await sender.Send(new GetUserDiscountQuery(id), token);
                return Results.Ok(new GetUserDiscountResponse(result.Discount ?? 0));
            })
            .WithDescription("Получение скидки пользователя")
            .WithDisplayName("Получение скидки пользователя")
            .RequireAnyPermission(PermissionCodes.USERS_DISCOUNT_GET);

        users.MapPost("/{userId}/mail/corporate", async (
                ISender sender,
                string userId,
                CreateMailForUserRequest request,
                CancellationToken token) =>
            {
                var command = new CreateMailForUserCommand(userId, request.MailBox, request.Password, request.Comment);
                var result = await sender.Send(command, token);
                string? uri = null;
                return Results.Created(uri, result.Adapt<CreateMailForUserResponse>());
            })
            .WithDescription("Создание корпоративной почты для пользователя")
            .WithDisplayName("Создание почты для пользователя")
            .RequireAnyPermission(PermissionCodes.USERS_MAILS_CREATE);

        users.MapGet("/{id:guid}/info", async (ISender sender, Guid id, CancellationToken token) =>
            {
                var result = await sender.Send(new GetUserFullInfoQuery(id), token);
                return Results.Ok(new GetUserFullInfoResponse(
                    result.User,
                    result.Emails,
                    result.Roles,
                    result.Permissions));
            })
            .WithDescription("Получение информации пользователя")
            .WithDisplayName("Получение информации пользователя")
            .RequireAnyPermission(PermissionCodes.USERS_INFO_GET);

        return users;
    }
}
