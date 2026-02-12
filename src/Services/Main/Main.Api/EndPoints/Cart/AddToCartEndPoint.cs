using System.Security.Claims;
using Abstractions.Interfaces;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.Cart.AddToCart;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Cart;

public record AddToCartRequest(int ArticleId, int Count);

public class AddToCartEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/cart", async (ISender sender, IUserContext user, AddToCartRequest request, CancellationToken cancellationToken) => 
            { 
                var command = new AddToCartCommand(user.UserId, request.ArticleId, request.Count);
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