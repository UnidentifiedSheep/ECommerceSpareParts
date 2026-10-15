using Extensions;
using OpenSearch.Client;
using Search.Application.Models.CatalogueSearch.Criteria;
using Search.Entities;
using Search.Persistence.Interfaces;

namespace Search.Persistence.Queries.ProductFilters;

public sealed class ProductWeightFilterProvider : IProductSearchFilterProvider
{
	public IEnumerable<QueryContainer> Build(ProductSearchCriteria criteria)
	{
		if (criteria.Weight is { } weight)
			yield return ProductRangeQueryBuilder.Build(Infer.Field<Product>(product => product.Weight!.WeightKg), weight.Range, 1m.ToKg(weight.Unit));
	}
}
