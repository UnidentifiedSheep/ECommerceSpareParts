using Contracts.Products;
using Extensions;
using System.Linq.Expressions;
using Search.Entities;

namespace Search.Application.Models.ProductUpdates;

public sealed record ProductDimensionsUpdate : ProductFieldUpdate
{
	public ProductDimensions? Dimensions { get; init; }

	public static ProductDimensionsUpdate From(ProductDimensionsSnapshot product, DateTime updatedAt)
	{
		var size = product.Dimensions;
		return new ProductDimensionsUpdate
		{
			ProductId = product.ProductId,
			UpdatedAt = updatedAt,
			Dimensions = size is null ? null : new ProductDimensions
			{
				Length = size.Length,
				Width = size.Width,
				Height = size.Height,
				Unit = size.Unit,
				LengthM = size.Length.ToMeters(size.Unit),
				WidthM = size.Width.ToMeters(size.Unit),
				HeightM = size.Height.ToMeters(size.Unit),
				VolumeM3 = DimensionExtensions.ToCubicMeters(size.Length, size.Width, size.Height, size.Unit)
			}
		};
	}

	public override Expression<Func<Product, DateTime?>> GetUpdatedAtField() => product => product.DimensionsUpdatedAt;
}
