using System.Security.Claims;
using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Cart.DeleteFromCart;
using Main.Core.Enums;
using MediatR;
using Security.Extensions;

namespace Main.Api.EndPoints.Cart;

public class DeleteFromCartEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/cart/{articleId}", async (ISender sender, ClaimsPrincipal user, int articleId, 
            CancellationToken cancellationToken) =>
        {
            if (!user.GetUserId(out var userId)) return Results.Unauthorized();
            
            var command = new DeleteFromCartCommand(userId, articleId);
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