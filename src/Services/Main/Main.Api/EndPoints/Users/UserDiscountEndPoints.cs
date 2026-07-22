using Api.Common.Extensions;
using Enums;
using Main.Application.Handlers.Users.ChangeUserDiscount;
using Main.Application.Handlers.Users.GetUserDiscount;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record ChangeDiscountForUserRequest(decimal NewDiscountRate);

public static class UserDiscountEndPoints
{
    public static RouteGroupBuilder MapUserDiscountEndPoints(this RouteGroupBuilder users)
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

        return users;
    }
}
