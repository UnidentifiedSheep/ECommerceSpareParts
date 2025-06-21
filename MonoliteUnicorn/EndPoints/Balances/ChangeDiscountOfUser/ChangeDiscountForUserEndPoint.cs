using Carter;
using Core.StaticFunctions;
using MediatR;

namespace MonoliteUnicorn.EndPoints.Balances.ChangeDiscountOfUser;

public record ChangeDiscountForUserRequest( decimal NewDiscount);

public class ChangeDiscountForUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/users/{userId}/discount/",
                async (ISender sender, string userId, ChangeDiscountForUserRequest request, CancellationToken cancellationToken) =>
                {
                    var command = new ChangeDiscountForUserCommand(userId, request.NewDiscount);
                    await sender.Send(command, cancellationToken);
                    return Results.Ok();
                }).WithGroup("Balances")
            .RequireAuthorization("AM")
            .WithDescription("Изменение скидки пользователя")
            .WithDisplayName("Поменять скидку");
    }
}