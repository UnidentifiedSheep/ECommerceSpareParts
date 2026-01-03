using System.Security.Claims;
using Api.Common.Extensions;
using Carter;
using Main.Application.Handlers.Cart.AddToCart;
using Main.Core.Enums;
using MediatR;
using Security.Extensions;

namespace Main.Api.EndPoints.Cart;

public record AddToCartRequest(int ArticleId, int Count);

public class AddToCartEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/cart", async (ISender sender, ClaimsPrincipal user, AddToCartRequest request, CancellationToken cancellationToken) =>
        {
            if (!user.GetUserId(out var userId)) return Results.Unauthorized();
            var command = new AddToCartCommand(userId, request.ArticleId, request.Count);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).WithTags("Cart")
        .WithDescription("Добавление позиции в корзину")
        .WithDisplayName("Добавление позиции в корзину")
        .Produces(204)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(400)
        .RequireAnyPermission(PermissionCodes.CART_ADD);
    }
}