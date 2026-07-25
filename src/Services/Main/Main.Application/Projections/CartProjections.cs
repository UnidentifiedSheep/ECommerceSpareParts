using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Main.Application.Dtos.Cart;
using Main.Application.Dtos.Product;
using Main.Entities.Cart;
using Main.Entities.Product;

namespace Main.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class CartItemDtoProjectionProvider
    : IProjectionProvider<Cart, CartItemDto>
{
    public CartItemDtoProjectionProvider(
        IProjectionProvider<Product, ProductDto> productProjection)
    {
        var productToDto = productProjection.Projection;

        Projection = x => new CartItemDto
        {
            Count = x.Count,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            ProductId = x.ProductId,
            Product = productToDto.Invoke(x.Product)
        };
    }

    public Expression<Func<Cart, CartItemDto>> Projection { get; }
}
