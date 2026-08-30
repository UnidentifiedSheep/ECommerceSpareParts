using System.Linq.Expressions;
using Application.Common.Interfaces.Projections;
using Attributes;
using LinqKit;
using Search.Application.Dtos.Products;
using Search.Entities;

namespace Search.Application.Projections;

[Lifetime(Lifetime.Singleton)]
public sealed class
	ProductDimensionsDtoProjectionProvider : ProjectionProviderBase<ProductDimensions, ProductDimensionsDto>
{
	public override Expression<Func<ProductDimensions, ProductDimensionsDto>> Projection { get; } =
		dimensions => new ProductDimensionsDto
		{
			Length = dimensions.Length,
			Width = dimensions.Width,
			Height = dimensions.Height,
			Unit = dimensions.Unit.ToString(),
			VolumeM3 = dimensions.VolumeM3
		};
}

[Lifetime(Lifetime.Singleton)]
public sealed class
	ProductWeightDtoProjectionProvider : ProjectionProviderBase<ProductWeight, ProductWeightDto>
{
	public override Expression<Func<ProductWeight, ProductWeightDto>> Projection { get; } = weight =>
		new ProductWeightDto
		{
			Value = weight.Value,
			Unit = weight.Unit.ToString(),
			WeightKg = weight.WeightKg
		};
}

[Lifetime(Lifetime.Singleton)]
public sealed class ProductDtoProjectionProvider : ProjectionProviderBase<Product, ProductDto>
{
	public ProductDtoProjectionProvider(
		IProjectionProvider<ProductDimensions, ProductDimensionsDto> dimensionsProjection,
		IProjectionProvider<ProductWeight, ProductWeightDto> weightProjection)
	{
		var dimensionsToDto = dimensionsProjection.Projection;
		var weightToDto = weightProjection.Projection;

		Projection = product => new ProductDto
		{
			Id = product.Id,
			Sku = product.Sku,
			Name = product.Name,
			ProducerId = product.ProducerId,
			Dimensions = product.Dimensions == null ? null : dimensionsToDto.Invoke(product.Dimensions),
			Weight = product.Weight == null ? null : weightToDto.Invoke(product.Weight),
			Stock = product.Stock,
			Indicator = product.Indicator
		};
	}

	public override Expression<Func<Product, ProductDto>> Projection { get; }
}
