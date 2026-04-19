using System.Linq.Expressions;
using Main.Abstractions.Dtos.Amw.Articles;
using Main.Entities.Product;

namespace Main.Application.Handlers.Currencies.Projections;

public static class ProductProjections
{
    public static Expression<Func<Product, ProductDto>> ToDto =
        x => new ProductDto
        {
            Id = x.Id,
            Name = x.Name,
            Sku = x.Sku,
            Description = x.Description,
            Stock = x.Stock,
            ProducerId = x.ProducerId,
            ProducerName = x.Producer.Name,
            Indicator = x.Indicator,
            Images = x.Images.Select(z => z.Path).ToList(),
        };
}