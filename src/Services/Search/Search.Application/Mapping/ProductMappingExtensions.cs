using Search.Application.Dtos.Products;
using Search.Entities;

namespace Search.Application.Mapping;

public static class ProductMappingExtensions
{
    public static ProductDto ToProductDto(this Product product)
        => new()
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            ProducerId = product.ProducerId,
            Dimensions = product.Dimensions?.ToProductDimensionsDto(),
            Weight = product.Weight?.ToProductWeightDto(),
            Stock = product.Stock
        };

    private static ProductDimensionsDto ToProductDimensionsDto(this ProductDimensions dimensions)
        => new()
        {
            Length = dimensions.Length,
            Width = dimensions.Width,
            Height = dimensions.Height,
            Unit = dimensions.Unit.ToString(),
            VolumeM3 = dimensions.VolumeM3
        };

    private static ProductWeightDto ToProductWeightDto(this ProductWeight weight)
        => new()
        {
            Value = weight.Value,
            Unit = weight.Unit.ToString(),
            WeightKg = weight.WeightKg
        };
}
