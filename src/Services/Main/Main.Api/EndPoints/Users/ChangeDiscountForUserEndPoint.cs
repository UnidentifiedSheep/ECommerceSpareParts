using Carter;
using Main.Application.Handlers.Users.ChangeUserDiscount;
using MediatR;

namespace Main.Api.EndPoints.Users;

public record ChangeDiscountForUserRequest(decimal NewDiscount);

public class ChangeDiscountForUserEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/users/{userId}/discount/",
                async (ISender sender, Guid userId, ChangeDiscountForUserRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var command = new ChangeUserDiscountCommand(userId, request.NewDiscount);
                    await sender.Send(command, cancellationToken);
                    return Results.Ok();
                }).WithTags("Balances")
            .WithDescription("Изменение скидки пользователя")
            .WithDisplayName("Поменять скидку");
    }
}