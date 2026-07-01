using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Cart;
using Main.Entities.Cart;

namespace Main.Application.Projections;

public static class CartProjections
{
    public static readonly Expression<Func<Cart, CartItemDto>> ToCartItemDto =
        x => new CartItemDto
        {
            Count = x.Count,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            ProductId = x.ProductId,
            Product = ProductProjections.ToDto.Invoke(x.Product)
        };
}