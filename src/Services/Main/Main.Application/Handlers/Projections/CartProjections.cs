using System.Linq.Expressions;
using LinqKit;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Abstractions.Dtos.Cart;

namespace Main.Application.Handlers.Currencies.Projections;

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