using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.Users.GetUserDiscount;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record GetUserDiscountResponse(decimal Discount);

public class GetUserDiscountEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:guid}/discount", async (ISender sender, Guid id, CancellationToken token) =>
        {
            var query = new GetUserDiscountQuery(id);
            var result = await sender.Send(query, token);
            return Results.Ok(new GetUserDiscountResponse(result.Discount ?? 0));
        }).WithTags("Users")
        .WithDescription("Получение скидки пользователя")
        .WithDisplayName("Получение скидки пользователя")
        .RequireAnyPermission(PermissionCodes.USERS_DISCOUNT_GET);
    }
}