using Abstractions.Interfaces;
using Abstractions.Models;
using Api.Common.Extensions;
using Carter;
using Enums;
using Main.Application.Dtos.Cart;
using Main.Application.Handlers.Cart.AddToCart;
using Main.Application.Handlers.Cart.ChangeCartItemCount;
using Main.Application.Handlers.Cart.DeleteFromCart;
using Main.Application.Handlers.Cart.GetCartItems;
using MediatR;

namespace Main.Api.EndPoints.Cart;

public record AddToCartRequest(int ArticleId, int Count);

public record ChangeCartItemCountRequest(int NewCount);

public record GetCartItemsResponse(List<CartItemDto> CartItems);

public class CartEndPoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var cart = app.MapGroup("/cart")
            .WithTags("Cart");

        cart.MapPost("", async (
                ISender sender,
                IUserContext user,
                AddToCartRequest request,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new AddToCartCommand(user.UserId, request.ArticleId, request.Count), cancellationToken);
                return Results.NoContent();
            })
            .WithDescription("Добавление позиции в корзину")
            .WithDisplayName("Добавление позиции в корзину")
            .Produces(204)
            .Produces(401)
            .Produces(403)
            .Produces(404)
            .Produces(400)
            .RequireAnyPermission(PermissionCodes.CART_ADD);

        cart.MapPatch("/{articleId}", async (
                ISender sender,
                IUserContext user,
                int articleId,
                ChangeCartItemCountRequest request,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(
                    new ChangeCartItemCountCommand(user.UserId, articleId, request.NewCount),
                    cancellationToken);
                return Results.NoContent();
            })
            .WithDescription("Редактирование позиции в корзине")
            .WithDisplayName("Редактирование позиции в корзине")
            .Produces(204)
            .Produces(401)
            .Produces(403)
            .Produces(404)
            .Produces(400)
            .RequireAnyPermission(PermissionCodes.CART_UPDATE);

        cart.MapDelete("/{articleId}", async (
                ISender sender,
                IUserContext user,
                int articleId,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteFromCartCommand(user.UserId, articleId), cancellationToken);
                return Results.NoContent();
            })
            .WithDescription("Удалить позицию из корзины")
            .WithDisplayName("Удалить позицию из корзины")
            .Produces(204)
            .Produces(401)
            .Produces(403)
            .Produces(404)
            .Produces(400)
            .RequireAnyPermission(PermissionCodes.CART_DELETE);

        cart.MapGet("", async (
                ISender sender,
                IUserContext user,
                int page,
                int limit,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetCartItemsQuery(user.UserId, new Pagination(page, limit)),
                    cancellationToken);
                return Results.Ok(new GetCartItemsResponse(result.CartItems));
            })
            .WithDescription("Получение позиций корзины")
            .WithDisplayName("Получение позиций корзины")
            .Produces(200)
            .RequireAnyPermission(PermissionCodes.CART_GET);
    }
}
