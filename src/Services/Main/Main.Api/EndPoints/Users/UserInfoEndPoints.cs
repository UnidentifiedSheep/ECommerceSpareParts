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
        users.MapPatch("/{userId:guid}/discount/", async (
                ISender sender,
                Guid userId,
                ChangeDiscountForUserRequest request,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new ChangeUserDiscountCommand(userId, request.NewDiscountRate), cancellationToken);
                return Results.Ok();
            })
            .WithName("ChangeUserDiscount")
            .WithSummary("Изменить скидку пользователя")
            .WithDescription("Изменение скидки пользователя")
            .WithDisplayName("Поменять скидку")
            .Accepts<ChangeDiscountForUserRequest>(false, "application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.USERS_DISCOUNT_CREATE);

        users.MapGet("/{id:guid}/discount", async (ISender sender, Guid id, CancellationToken token) =>
            {
                var result = await sender.Send(new GetUserDiscountQuery(id), token);
                return Results.Ok(new GetUserDiscountResponse(result.Discount ?? 0));
            })
            .WithName("GetUserDiscount")
            .WithSummary("Получить скидку пользователя")
            .WithDescription("Получение скидки пользователя")
            .WithDisplayName("Получение скидки пользователя")
            .Produces<GetUserDiscountResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
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
            .WithName("CreateUserCorporateMail")
            .WithSummary("Создать корпоративную почту пользователя")
            .WithDescription("Создание корпоративной почты для пользователя")
            .WithDisplayName("Создание почты для пользователя")
            .Accepts<CreateMailForUserRequest>(false, "application/json")
            .Produces<CreateMailForUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
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
            .WithName("GetUserFullInfo")
            .WithSummary("Получить полную информацию пользователя")
            .WithDescription("Получение информации пользователя")
            .WithDisplayName("Получение информации пользователя")
            .Produces<GetUserFullInfoResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.USERS_INFO_GET);

        return users;
    }
}
