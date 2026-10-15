using OpenSearch.Client;
using Search.Application.Models.CatalogueSearch.Criteria;
using Search.Entities;
using Search.Persistence.Interfaces;

namespace Search.Persistence.Queries.ProductFilters;

public sealed class ProductStockFilterProvider : IProductSearchFilterProvider
{
	public IEnumerable<QueryContainer> Build(ProductSearchCriteria criteria)
	{
		if (criteria.Stock is { } range)
			yield return ProductRangeQueryBuilder.Build(
				Infer.Field<Product>(product => product.Stock),
				range);
	}
}
