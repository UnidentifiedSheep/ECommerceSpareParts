using Search.Application.Dtos.Products;
using Search.Entities;

namespace Search.Application.Mapping;

public static class ProductMappingExtensions
{
    public static ProductDto ToProductDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            ProducerId = product.ProducerId,
            Dimensions = product.Dimensions?.ToProductDimensionsDto(),
            Weight = product.Weight?.ToProductWeightDto(),
            Stock = product.Stock,
            Indicator = product.Indicator
        };
    }

    private static ProductDimensionsDto ToProductDimensionsDto(this ProductDimensions dimensions)
    {
        return new ProductDimensionsDto
        {
            Length = dimensions.Length,
            Width = dimensions.Width,
            Height = dimensions.Height,
            Unit = dimensions.Unit.ToString(),
            VolumeM3 = dimensions.VolumeM3
        };
    }

    private static ProductWeightDto ToProductWeightDto(this ProductWeight weight)
    {
        return new ProductWeightDto
        {
            Value = weight.Value,
            Unit = weight.Unit.ToString(),
            WeightKg = weight.WeightKg
        };
    }
}