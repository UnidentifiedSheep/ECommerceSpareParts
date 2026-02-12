using System.Security.Claims;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.Cart.DeleteFromCart;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Cart;

public class DeleteFromCartEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/cart/{articleId}", async (ISender sender, IUserContext user, int articleId, 
            CancellationToken cancellationToken) =>
            {
                var command = new DeleteFromCartCommand(user.UserId, articleId);
                await sender.Send(command, cancellationToken);
                return Results.NoContent();
            }).WithTags("Cart")
            .WithDescription("Удалить позицию из корзины")
            .WithDisplayName("Удалить позицию из корзины")
            .Produces(204)
            .Produces(401)
            .Produces(403)
            .Produces(404)
            .Produces(400)
            .RequireAnyPermission(PermissionCodes.CART_DELETE);
    }
}