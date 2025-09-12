using Application.Handlers.Users.ChangeUserDiscount;
using Carter;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Balances;

public record ChangeDiscountForUserRequest(decimal NewDiscount);

public class ChangeDiscountForUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/users/{userId}/discount/",
                async (ISender sender, string userId, ChangeDiscountForUserRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var command = new ChangeUserDiscountCommand(userId, request.NewDiscount);
                    await sender.Send(command, cancellationToken);
                    return Results.Ok();
                }).WithTags("Balances")
            .RequireAuthorization("AM")
            .WithDescription("Изменение скидки пользователя")
            .WithDisplayName("Поменять скидку");
    }
}