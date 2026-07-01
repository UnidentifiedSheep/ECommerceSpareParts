using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Enums;
using Main.Application.Dtos.Balances;
using Main.Application.Dtos.Currencies;
using Main.Application.Handlers.Balance;
using Main.Application.Handlers.Balance.UpdateUserFinancialProfile;
using Main.Application.Handlers.Users.ChangeUserDiscount;
using Main.Application.Handlers.Users.GetUserDiscount;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record ChangeDiscountForUserRequest(decimal NewDiscountRate);

public record GetUserFinancialInfoResponse
{
    [JsonPropertyName("financialProfile")]
    public required UserFinancialProfileDto? FinancialProfile { get; init; }

    [JsonPropertyName("baseCurrency")]
    public required CurrencyDto BaseCurrency { get; init; }

    [JsonPropertyName("balances")]
    public required IEnumerable<UserBalanceDto> Balances { get; init; }
}

public record UpdateUserFinancialInfoRequest
{
    [JsonPropertyName("financialProfile")]
    public required PatchUserFinancialProfileDto FinancialProfile { get; init; }
}

public record UpdateUserFinancialInfoResponse
{
    [JsonPropertyName("financialProfile")]
    public required UserFinancialProfileDto FinancialProfile { get; init; }
}

public static class UserFinancialEndPoints
{
    public static RouteGroupBuilder MapUserFinancialEndPoints(this RouteGroupBuilder users)
    {
        users.MapPatch(
                "/{userId:guid}/discount/",
                async (
                    ISender sender,
                    Guid userId,
                    ChangeDiscountForUserRequest request,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(
                        new ChangeUserDiscountCommand(userId, request.NewDiscountRate),
                        cancellationToken);
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

        users.MapGet(
                "/{id:guid}/discount",
                async (
                    ISender sender,
                    Guid id,
                    CancellationToken token) =>
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

        users.MapGet(
                "/{id:guid}/finances",
                async (
                    ISender sender,
                    Guid id,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(new GetUserFinancialInfoQuery(id), token);
                    return Results.Ok(
                        new GetUserFinancialInfoResponse
                        {
                            Balances = result.Balances,
                            BaseCurrency = result.BaseCurrency,
                            FinancialProfile = result.FinancialProfile
                        });
                })
            .WithName("GetUserFinancialInfo")
            .WithSummary("Получить информацию по финанцам пользователя")
            .WithDescription("Получение финансовой информации пользователя")
            .WithDisplayName("Получение финансовой информации пользователя")
            .Produces<GetUserFinancialInfoResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.BALANCES_FINANCES_GET);

        users.MapPatch(
                "/{id:guid}/finances",
                async (
                    ISender sender,
                    UpdateUserFinancialInfoRequest request,
                    Guid id,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(
                        new UpdateUserFinancialProfileCommand(
                            id,
                            request.FinancialProfile),
                        token);
                    return Results.Ok(
                        new UpdateUserFinancialInfoResponse
                        {
                            FinancialProfile = result.Profile
                        });
                })
            .WithName("UpdateUserFinancialInfo")
            .WithSummary("Обновление финансового профиля пользователя")
            .WithDescription("Обновление финансового профиля пользователя")
            .WithDisplayName("Обновление финансового профиля пользователя")
            .Produces<UpdateUserFinancialInfoResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.BALANCES_FINANCES_UPDATE);

        return users;
    }
}