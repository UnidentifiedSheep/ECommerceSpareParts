using System.Linq.Expressions;
using LinqKit;
using Main.Application.Dtos.Amw.ArticleCoefficients;
using Main.Application.Dtos.Product;
using Main.Entities.Product;

namespace Main.Application.Handlers.Projections;

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

    public static Expression<Func<Entities.Product.ProductCoefficient, ProductCoefficientDto>> ToProductCoefficientDto =
        x => new ProductCoefficientDto
        {
            ProductId = x.ProductId,
            ValidTill = x.ValidTill,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            Coefficient = CoefficientProjections.ToDto.Invoke(x.Coefficient),
        };
}