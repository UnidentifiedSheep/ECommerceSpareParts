using System.Security.Claims;
using Abstractions.Interfaces;
using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Handlers.Cart.GetCartItems;
using Main.Abstractions.Dtos.Cart;
using Main.Enums;
using MediatR;

namespace Main.Api.EndPoints.Cart;

public record GetCartItemsResponse(List<CartItemDto> CartItems);

public class GetCartItemsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/cart", async (ISender sender, IUserContext user, int page, int limit, 
                CancellationToken cancellationToken) =>
        {
            var query = new GetCartItemsQuery(user.UserId, new PaginationModel(page, limit));
            var result = await sender.Send(query, cancellationToken);
            var response = new GetCartItemsResponse(result.CartItems);

            return Results.Ok(response);
        }).WithTags("Cart")
        .WithDescription("Получение позиций корзины")
        .WithDisplayName("Получение позиций корзины")
        .Produces(200)
        .RequireAnyPermission(PermissionCodes.CART_GET);
    }
}