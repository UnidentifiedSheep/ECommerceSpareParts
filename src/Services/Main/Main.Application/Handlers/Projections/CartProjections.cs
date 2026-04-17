using System.Linq.Expressions;
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
            Product = new ProductDto
            {
                Id = x.Product.Id,
                Name = x.Product.Name,
                Sku = x.Product.Sku,
                Description = x.Product.Description,
                Stock = x.Product.Stock,
                ProducerId = x.Product.ProducerId,
                ProducerName = x.Product.Producer.Name,
                Indicator = x.Product.Indicator,
                Images = x.Product.Images.Select(z => z.Path).ToList(),
            }
        };
    
    
}