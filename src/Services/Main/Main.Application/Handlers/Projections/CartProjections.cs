using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Cart;

namespace Main.Application.Handlers.Projections;

public static class CartProjections
{
    public static Expression<Func<Entities.Cart.Cart, CartItemDto>> ToCartItemDto =
        x => new CartItemDto
        {
            Count = x.Count,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            ProductId = x.ProductId,
            Product = ProductProjections.ToDto.Invoke(x.Product),
        };
    
    
}