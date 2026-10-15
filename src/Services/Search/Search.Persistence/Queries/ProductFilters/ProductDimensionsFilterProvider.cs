using Extensions;
using OpenSearch.Client;
using Search.Application.Models.CatalogueSearch.Criteria;
using Search.Entities;
using Search.Persistence.Extensions;
using Search.Persistence.Interfaces;

namespace Search.Persistence.Queries.ProductFilters;

public sealed class ProductDimensionsFilterProvider : IProductSearchFilterProvider
{
	public IEnumerable<QueryContainer> Build(ProductSearchCriteria criteria)
	{
		if (criteria.Dimensions is not { } dimensions) return [];
		var scale = dimensions.Unit.GetMetersScale();
		return new List<QueryContainer>()
			.ApplyWhenNotNull(dimensions.Length, length =>
				ProductRangeQueryBuilder.Build(Infer.Field<Product>(product => product.Dimensions!.LengthM), length, scale))
			.ApplyWhenNotNull(dimensions.Width, width =>
				ProductRangeQueryBuilder.Build(Infer.Field<Product>(product => product.Dimensions!.WidthM), width, scale))
			.ApplyWhenNotNull(dimensions.Height, height =>
				ProductRangeQueryBuilder.Build(Infer.Field<Product>(product => product.Dimensions!.HeightM), height, scale))
			.ApplyWhenNotNull(dimensions.Volume, volume =>
				ProductRangeQueryBuilder.Build(Infer.Field<Product>(product => product.Dimensions!.VolumeM3), volume, scale * scale * scale));
	}
}
